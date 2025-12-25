using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class ConversationSession : BaseEntity
{
    public Guid ConversationId { get; set; }
    public string WebSocketId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public int AudioChunksSent { get; set; } = 0;
    public int AudioChunksReceived { get; set; } = 0;
    public int EstimatedCreditsUsed { get; set; } = 0;
    public string EndReason { get; set; } = string.Empty; // Normal, Timeout, CreditLimit, UserDisconnect, Error
    
    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
}
