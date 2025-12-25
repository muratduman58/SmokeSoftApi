using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class Conversation : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid AIIdentityId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    
    // AI Call Tracking
    public bool IsAICall { get; set; } = false;
    public DateTime? CallStartedAt { get; set; }
    public DateTime? CallEndedAt { get; set; }
    public int MinutesUsed { get; set; } = 0;

    // Navigation properties
    public User User { get; set; } = null!;
    public AIIdentity AIIdentity { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
