namespace SmokeSoft.Shared.DTOs.ShadowGuard;

/// <summary>
/// ElevenLabs hazır ses bilgileri
/// </summary>
public class PresetVoiceDto
{
    public string VoiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Accent { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
    public string UseCase { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty;
    public List<string> AvailableForTiers { get; set; } = new();
}

/// <summary>
/// ElevenLabs API'den gelen ses listesi response
/// </summary>
public class VoicesResponse
{
    public List<VoiceInfo> Voices { get; set; } = new();
}

/// <summary>
/// ElevenLabs API'den gelen tek bir ses bilgisi
/// </summary>
public class VoiceInfo
{
    public string VoiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public VoiceLabels Labels { get; set; } = new();
    public string PreviewUrl { get; set; } = string.Empty;
    public List<string> AvailableForTiers { get; set; } = new();
}

/// <summary>
/// Ses etiketleri
/// </summary>
public class VoiceLabels
{
    public string Accent { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string UseCase { get; set; } = string.Empty;
}
