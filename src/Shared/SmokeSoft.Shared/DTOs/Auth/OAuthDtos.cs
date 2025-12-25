namespace SmokeSoft.Shared.DTOs.Auth;

// Device DTOs
public class DeviceInfoRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // iOS, Android, Web
    public string PlatformVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public string? FcmToken { get; set; }
}

public class DeviceDto
{
    public Guid Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string PlatformVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public DateTime LastSeenAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// OAuth DTOs
public class OAuthLoginRequest
{
    public string Provider { get; set; } = string.Empty; // Google, Apple
    public string IdToken { get; set; } = string.Empty; // ID token from OAuth provider
    public DeviceInfoRequest DeviceInfo { get; set; } = new();
}

public class OAuthProviderDto
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime LinkedAt { get; set; }
}

// Updated AuthResponse to include OAuth info
public class OAuthAuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
    public bool IsNewUser { get; set; }
    public List<OAuthProviderDto> LinkedProviders { get; set; } = new();
}
