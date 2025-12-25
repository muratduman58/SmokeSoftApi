using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class UserOAuthProvider : BaseEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty; // Google, Apple
    public string ProviderUserId { get; set; } = string.Empty; // User ID from OAuth provider
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}
