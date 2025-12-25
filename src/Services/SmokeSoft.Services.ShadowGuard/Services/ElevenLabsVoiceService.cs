using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace SmokeSoft.Services.ShadowGuard.Services;

public class ElevenLabsVoiceService : IElevenLabsVoiceService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<ElevenLabsVoiceService> _logger;
    private const string BaseUrl = "https://api.elevenlabs.io/v1";

    public ElevenLabsVoiceService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<ElevenLabsVoiceService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
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

            var presetVoices = result.Voices
                .Where(v => v.Category == "premade") // Sadece hazır sesler
                .Select(v => new SmokeSoft.Shared.DTOs.ShadowGuard.PresetVoiceDto
                {
                    VoiceId = v.VoiceId,
                    Name = v.Name,
                    Gender = v.Labels?.Gender ?? "unknown",
                    Accent = v.Labels?.Accent ?? "unknown",
                    Description = v.Labels?.Description ?? "",
                    Age = v.Labels?.Age ?? "unknown",
                    UseCase = v.Labels?.UseCase ?? "",
                    PreviewUrl = v.PreviewUrl,
                    AvailableForTiers = v.AvailableForTiers ?? new List<string>()
                })
                .ToList();

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
