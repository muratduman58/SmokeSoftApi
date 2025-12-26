namespace SmokeSoft.Shared.DTOs.Auth;

/// <summary>
/// Request to register an anonymous device/user
/// </summary>
public record RegisterAnonymousRequest(
    string DeviceId,
    string? FcmToken,
    DeviceInfoDto DeviceInfo
);

/// <summary>
/// Device information DTO
/// </summary>
public record DeviceInfoDto(
    string DeviceName,
    string DeviceModel,
    string Platform,
    string PlatformVersion,
    string AppVersion
);
