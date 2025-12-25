using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;

    // Subscription
    public bool IsPro { get; set; } = false;
    public DateTime? ProExpiresAt { get; set; }
    
    // AI Minutes (only for AI calls)
    public int TotalAIMinutes { get; set; } = 0;
    public int UsedAIMinutes { get; set; } = 0;
    public int RemainingAIMinutes => TotalAIMinutes - UsedAIMinutes;
    
    // AI Slots (AI Identity creation limit)
    public int TotalAISlots { get; set; } = 0;
    public int UsedAISlots { get; set; } = 0;
    public int RemainingAISlots => TotalAISlots - UsedAISlots;
    public bool HasUnlimitedAISlots { get; set; } = false;

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<AIIdentity> AIIdentities { get; set; } = new List<AIIdentity>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<UserOAuthProvider> OAuthProviders { get; set; } = new List<UserOAuthProvider>();
    public UserProfile? UserProfile { get; set; }
}
