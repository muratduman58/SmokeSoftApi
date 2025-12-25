using SmokeSoft.Shared.DTOs.Auth;

namespace SmokeSoft.Shared.DTOs.ShadowGuard;

public class VerifyPurchaseRequest
{
    public string Platform { get; set; } = string.Empty; // iOS, Android
    public string Receipt { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
}

public class PurchaseVerificationDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PackageInfoDto? Package { get; set; }
    public UserSubscriptionDto? Subscription { get; set; }
}

public class PackageInfoDto
{
    public string Name { get; set; } = string.Empty;
    public int MinutesAdded { get; set; }
    public int SlotsAdded { get; set; }
    public bool UnlimitedSlots { get; set; }
}
