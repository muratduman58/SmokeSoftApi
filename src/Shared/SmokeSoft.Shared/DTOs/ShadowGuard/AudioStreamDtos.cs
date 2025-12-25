namespace SmokeSoft.Shared.DTOs.ShadowGuard;

// Audio streaming DTOs
public class AudioStreamMessage
{
    public Guid ConversationId { get; set; }
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public string AudioFormat { get; set; } = "wav"; // wav, mp3, m4a, etc.
    public int SampleRate { get; set; } = 16000; // Hz
    public int Channels { get; set; } = 1; // Mono
    public int BitDepth { get; set; } = 16;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsEndOfStream { get; set; } = false;
}

public class AudioStreamResponse
{
    public Guid ConversationId { get; set; }
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public string AudioFormat { get; set; } = "wav";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsEndOfStream { get; set; } = false;
}

public class AudioStreamStatus
{
    public bool IsRecording { get; set; }
    public bool IsProcessing { get; set; }
    public string? Message { get; set; }
    public int BytesReceived { get; set; }
}

// WebSocket message types for voice
public class VoiceWebSocketMessage
{
    public string Type { get; set; } = string.Empty; // "audio", "start_recording", "stop_recording", "status", "error"
    public object? Data { get; set; }
}
