using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class Device : BaseEntity
{
    public string DeviceId { get; set; } = string.Empty; // Unique device identifier from client
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // iOS, Android, Web
    public string PlatformVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public string? FcmToken { get; set; } // Firebase Cloud Messaging token
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // User association (nullable - device can exist before user login)
    public Guid? UserId { get; set; }
    public User? User { get; set; }
}
