using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class VoiceSlot : BaseEntity
{
    public Guid AIIdentityId { get; set; }
    public string ElevenLabsVoiceId { get; set; } = string.Empty;
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedFromElevenLabsAt { get; set; }
    
    // Navigation properties
    public AIIdentity AIIdentity { get; set; } = null!;
}
