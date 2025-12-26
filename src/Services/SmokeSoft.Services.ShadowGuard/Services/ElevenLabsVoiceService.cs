using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace SmokeSoft.Services.ShadowGuard.Services;

public class ElevenLabsVoiceService : IElevenLabsVoiceService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<ElevenLabsVoiceService> _logger;
    private readonly ILocalizationService _localization;
    private const string BaseUrl = "https://api.elevenlabs.io/v1";

    public ElevenLabsVoiceService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<ElevenLabsVoiceService> logger,
        ILocalizationService localization)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _localization = localization;
    }

    public async Task<string> CloneVoiceAsync(
        string name,
        Stream audioStream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _config["ElevenLabs:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("ElevenLabs API key not configured");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/voices/add");
            request.Headers.Add("xi-api-key", apiKey);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(name), "name");
            content.Add(new StreamContent(audioStream), "files", "voice.mp3");

            request.Content = content;

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<VoiceCloneResponse>(cancellationToken);
            
            if (result == null || string.IsNullOrEmpty(result.VoiceId))
            {
                throw new InvalidOperationException("Failed to clone voice: Invalid response");
            }

            _logger.LogInformation("Voice cloned successfully: {VoiceId} for {Name}", result.VoiceId, name);
            
            return result.VoiceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clone voice for {Name}", name);
            throw;
        }
    }

    public async Task DeleteVoiceAsync(string voiceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _config["ElevenLabs:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("ElevenLabs API key not configured");
            }

            var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/voices/{voiceId}");
            request.Headers.Add("xi-api-key", apiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Voice deleted successfully: {VoiceId}", voiceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete voice {VoiceId}", voiceId);
            throw;
        }
    }

    public async Task<Stream> TextToSpeechStreamAsync(
        string voiceId,
        string text,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _config["ElevenLabs:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("ElevenLabs API key not configured");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/text-to-speech/{voiceId}/stream");
            request.Headers.Add("xi-api-key", apiKey);

            var body = new
            {
                text = text,
                model_id = "eleven_multilingual_v2",
                voice_settings = new
                {
                    stability = 0.5,
                    similarity_boost = 0.75
                }
            };

            request.Content = JsonContent.Create(body);

            var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            // Track cost
            if (response.Headers.TryGetValues("x-character-count", out var charCountValues))
            {
                var charCount = charCountValues.FirstOrDefault();
                var requestId = response.Headers.TryGetValues("request-id", out var requestIdValues)
                    ? requestIdValues.FirstOrDefault()
                    : "unknown";

                _logger.LogInformation("TTS: {CharCount} chars, RequestId: {RequestId}", charCount, requestId);
            }

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate TTS for voice {VoiceId}", voiceId);
            throw;
        }
    }

    public async Task<ClientWebSocket> ConnectWebSocketAsync(
        string voiceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _config["ElevenLabs:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("ElevenLabs API key not configured");
            }

            var ws = new ClientWebSocket();
            ws.Options.SetRequestHeader("xi-api-key", apiKey);

            var wsUrl = $"wss://api.elevenlabs.io/v1/text-to-speech/{voiceId}/stream-input?model_id=eleven_multilingual_v2&enable_logging=false";
            
            await ws.ConnectAsync(new Uri(wsUrl), cancellationToken);

            _logger.LogInformation("Connected to ElevenLabs WebSocket for voice {VoiceId}", voiceId);

            return ws;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to ElevenLabs WebSocket for voice {VoiceId}", voiceId);
            throw;
        }
    }

    public async Task<List<SmokeSoft.Shared.DTOs.ShadowGuard.PresetVoiceDto>> GetPresetVoicesAsync(
        string languageCode = "tr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _config["ElevenLabs:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("ElevenLabs API key not configured");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/voices");
            request.Headers.Add("xi-api-key", apiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SmokeSoft.Shared.DTOs.ShadowGuard.VoicesResponse>(cancellationToken);

            if (result == null || result.Voices == null)
            {
                _logger.LogWarning("No voices returned from ElevenLabs API");
                return new List<SmokeSoft.Shared.DTOs.ShadowGuard.PresetVoiceDto>();
            }

            var presetVoices = new List<SmokeSoft.Shared.DTOs.ShadowGuard.PresetVoiceDto>();
            
            foreach (var voice in result.Voices.Where(v => v.Category == "premade"))
            {
                var gender = await _localization.GetAsync(
                    $"gender.{voice.Labels?.Gender?.ToLower() ?? "unknown"}", 
                    languageCode);
                    
                var accent = await _localization.GetAsync(
                    $"accent.{voice.Labels?.Accent?.ToLower() ?? "unknown"}", 
                    languageCode);
                    
                var age = await _localization.GetAsync(
                    $"age.{voice.Labels?.Age?.ToLower()?.Replace(" ", "_") ?? "unknown"}", 
                    languageCode);
                    
                var useCase = string.IsNullOrEmpty(voice.Labels?.UseCase) 
                    ? "" 
                    : await _localization.GetAsync(
                        $"usecase.{voice.Labels.UseCase.ToLower()}", 
                        languageCode);

                presetVoices.Add(new SmokeSoft.Shared.DTOs.ShadowGuard.PresetVoiceDto
                {
                    VoiceId = voice.VoiceId,
                    Name = voice.Name,
                    Gender = gender,
                    Accent = accent,
                    Description = voice.Labels?.Description ?? "",
                    Age = age,
                    UseCase = useCase,
                    PreviewUrl = voice.PreviewUrl,
                    AvailableForTiers = voice.AvailableForTiers ?? new List<string>()
                });
            }

            _logger.LogInformation("Retrieved {Count} preset voices from ElevenLabs", presetVoices.Count);

            return presetVoices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get preset voices from ElevenLabs");
            throw;
        }
    }


    private class VoiceCloneResponse
    {
        public string VoiceId { get; set; } = string.Empty;
        public bool RequiresVerification { get; set; }
    }
}
