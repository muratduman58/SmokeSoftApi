using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class PurchaseVerification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Platform { get; set; } = string.Empty; // iOS, Android
    public string Receipt { get; set; } = string.Empty; // Store receipt/token
    public string ProductId { get; set; } = string.Empty; // com.smokesoft.starter
    public int MinutesGranted { get; set; }
    public int SlotsGranted { get; set; }
    public bool UnlimitedSlotsGranted { get; set; } = false;
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}
