namespace SmokeSoft.Shared.DTOs.ShadowGuard;

public class UploadScreenshotRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string ScreenType { get; set; } = string.Empty; // IncomingCall, Conversation
    
    // Device info
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string PlatformVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    
    // File will be sent as IFormFile in multipart/form-data
}

public class ScreenCustomizationDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string ScreenType { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty; // Full URL to access image
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ScreenCustomizationListDto
{
    public string ScreenType { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
