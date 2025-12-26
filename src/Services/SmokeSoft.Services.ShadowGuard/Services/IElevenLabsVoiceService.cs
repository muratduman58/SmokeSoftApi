namespace SmokeSoft.Services.ShadowGuard.Services;

public interface IElevenLabsVoiceService
{
    /// <summary>
    /// Clone a voice from an audio stream using ElevenLabs IVC (Instant Voice Cloning)
    /// </summary>
    Task<string> CloneVoiceAsync(string name, Stream audioStream, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a voice from ElevenLabs (for LRU cleanup)
    /// </summary>
    Task DeleteVoiceAsync(string voiceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate text-to-speech audio stream
    /// </summary>
    Task<Stream> TextToSpeechStreamAsync(string voiceId, string text, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Connect to ElevenLabs WebSocket for real-time conversation
    /// </summary>
    Task<System.Net.WebSockets.ClientWebSocket> ConnectWebSocketAsync(string voiceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get preset voices from ElevenLabs library
    /// </summary>
    Task<List<SmokeSoft.Shared.DTOs.ShadowGuard.PresetVoiceDto>> GetPresetVoicesAsync(string languageCode = "tr", CancellationToken cancellationToken = default);
}
