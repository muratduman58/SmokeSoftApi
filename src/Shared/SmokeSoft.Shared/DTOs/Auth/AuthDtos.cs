namespace SmokeSoft.Shared.DTOs.Auth;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; } // Nullable - anonim kullanıcılar için null
    public string? DisplayName { get; set; } // OAuth'tan veya kullanıcıdan
    public string? PhoneNumber { get; set; }
    public bool IsPro { get; set; }
    public DateTime? ProExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsAnonymous => string.IsNullOrEmpty(Email);
}

public class UserSubscriptionDto
{
    public bool IsPro { get; set; }
    public DateTime? ProExpiresAt { get; set; }
    public int RemainingAIMinutes { get; set; }
    public int RemainingAISlots { get; set; }
    public bool HasUnlimitedAISlots { get; set; }
}

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
