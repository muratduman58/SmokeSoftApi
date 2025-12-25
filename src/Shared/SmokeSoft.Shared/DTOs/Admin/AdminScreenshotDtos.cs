using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Shared.DTOs.Admin;

public class AdminScreenshotDto
{
    public Guid Id { get; set; }
    public string ScreenType { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Device Info
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string PlatformVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    
    // User Info
    public bool HasUser { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string UserType { get; set; } = string.Empty; // "Registered" or "Guest"
}

public class ScreenshotStatsDto
{
    public int TotalScreenshots { get; set; }
    public Dictionary<string, int> ByScreenType { get; set; } = new();
    public Dictionary<string, int> ByPlatform { get; set; } = new();
    public int RegisteredUsers { get; set; }
    public int GuestUsers { get; set; }
}

public class AdminScreenshotFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? ScreenType { get; set; }
    public string? Platform { get; set; }
    public bool? HasUser { get; set; }
}
