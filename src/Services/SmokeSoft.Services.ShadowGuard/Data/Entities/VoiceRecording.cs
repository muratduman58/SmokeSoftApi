using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class VoiceRecording : BaseEntity
{
    public Guid AIIdentityId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public int Duration { get; set; } // in seconds

    // Navigation properties
    public AIIdentity AIIdentity { get; set; } = null!;
}
