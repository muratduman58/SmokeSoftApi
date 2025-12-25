using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class CreditUsageLog : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? ConversationId { get; set; }
    public int CreditsUsed { get; set; }
    public string Operation { get; set; } = string.Empty; // TTS, STT, VoiceClone, VoiceGeneration
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = string.Empty; // JSON with additional info
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Conversation? Conversation { get; set; }
}
