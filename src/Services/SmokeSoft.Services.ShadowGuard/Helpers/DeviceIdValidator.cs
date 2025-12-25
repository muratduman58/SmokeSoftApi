namespace SmokeSoft.Services.ShadowGuard.Helpers;

public static class DeviceIdValidator
{
    private const int MinLength = 10;
    private const int MaxLength = 255;

    public static bool IsValid(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return false;

        // Check length
        if (deviceId.Length < MinLength || deviceId.Length > MaxLength)
            return false;

        // Check for valid characters (alphanumeric, hyphens, underscores)
        if (!System.Text.RegularExpressions.Regex.IsMatch(deviceId, @"^[a-zA-Z0-9\-_]+$"))
            return false;

        // Check for suspicious patterns (all same character)
        if (deviceId.Distinct().Count() < 3)
            return false;

        return true;
    }

    public static string GetValidationError(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return "Device ID is required";

        if (deviceId.Length < MinLength)
            return $"Device ID must be at least {MinLength} characters";

        if (deviceId.Length > MaxLength)
            return $"Device ID must not exceed {MaxLength} characters";

        if (!System.Text.RegularExpressions.Regex.IsMatch(deviceId, @"^[a-zA-Z0-9\-_]+$"))
            return "Device ID can only contain alphanumeric characters, hyphens, and underscores";

        if (deviceId.Distinct().Count() < 3)
            return "Device ID appears to be invalid";

        return string.Empty;
    }
}
