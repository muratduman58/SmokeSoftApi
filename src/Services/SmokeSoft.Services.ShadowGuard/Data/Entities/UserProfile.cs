using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? PreferredAIIdentityId { get; set; }
    public string Settings { get; set; } = "{}"; // JSON settings

    // Navigation properties
    public User User { get; set; } = null!;
    public AIIdentity? PreferredAIIdentity { get; set; }
}
