using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class AIIdentity : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GreetingStyle { get; set; } = string.Empty;
    public string Catchphrases { get; set; } = string.Empty; // JSON array stored as string
    public int Formality { get; set; } // 0-100
    public int Emotion { get; set; } // 0-100
    public int Verbosity { get; set; } // 0-100
    public string ExpertiseArea { get; set; } = string.Empty;
    public int SensitivityLevel { get; set; } // 0-100
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<VoiceRecording> VoiceRecordings { get; set; } = new List<VoiceRecording>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
