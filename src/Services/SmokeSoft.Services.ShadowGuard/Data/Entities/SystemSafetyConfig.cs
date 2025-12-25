using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class SystemSafetyConfig : BaseEntity
{
    // ElevenLabs Plan Configuration
    public string ElevenLabsPlanTier { get; set; } = "Free"; // Free, Starter, Creator, Pro, Scale, Business, Enterprise
    public decimal MonthlyPrice { get; set; } = 0;
    public int MonthlyCredits { get; set; } = 10000;
    public int CreditsUsed { get; set; } = 0;
    public int RemainingCredits => MonthlyCredits - CreditsUsed;
    public int EstimatedMinutes { get; set; } = 20;
    public int MinutesUsed { get; set; } = 0;
    public int RemainingMinutes => EstimatedMinutes - MinutesUsed;
    
    // Hard Limits (Admin-configurable)
    public int AbsoluteMaxConcurrentConnections { get; set; } = 50;
    public int AbsoluteMaxVoiceSlots { get; set; } = 200;
    public int AbsoluteMaxConversationMinutes { get; set; } = 60;
    public int AbsoluteMaxDailyMinutesPerUser { get; set; } = 120;
    public int AbsoluteMaxDailyCredits { get; set; } = 100000;
    
    // Alert Thresholds
    public decimal CreditWarningThreshold { get; set; } = 0.8m; // 80%
    public decimal CreditDangerThreshold { get; set; } = 0.95m; // 95%
    
    // Feature Toggles
    public bool EnableHardLimits { get; set; } = true;
    public bool EnableAutoStop { get; set; } = true;
    public bool EnableDailyLimits { get; set; } = true;
    
    // Alert Configuration
    public string AlertEmail { get; set; } = string.Empty;
    public string AlertPhone { get; set; } = string.Empty;
    public string SlackWebhookUrl { get; set; } = string.Empty;
    
    // Billing Period
    public DateTime PeriodStartDate { get; set; } = DateTime.UtcNow;
    public DateTime PeriodEndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    public bool AutoRenew { get; set; } = true;
    
    // Status
    public bool IsActive { get; set; } = true;
}
