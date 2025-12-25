using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class VoiceSample : BaseEntity
{
    public Guid AIIdentityId { get; set; }
    public string BlobUrl { get; set; } = string.Empty; // Azure Blob / S3 URL
    public string ParametersJson { get; set; } = string.Empty; // Voice generation parameters (Age, Gender, Accent)
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
    public long FileSizeBytes { get; set; }
    public int DurationSeconds { get; set; }
    
    // Navigation properties
    public AIIdentity AIIdentity { get; set; } = null!;
}
