using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class ScreenCustomization : BaseEntity
{
    public Guid? UserId { get; set; } // Nullable - can exist before user login
    public string DeviceId { get; set; } = string.Empty;
    public string ScreenType { get; set; } = string.Empty; // IncomingCall, Conversation
    public string ImagePath { get; set; } = string.Empty; // Relative path in project
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    
    // Device info at upload time
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string PlatformVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;

    // Navigation properties
    public User? User { get; set; }
}
