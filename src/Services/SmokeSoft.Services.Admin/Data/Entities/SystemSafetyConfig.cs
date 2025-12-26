using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.Admin.Data.Entities;

/// <summary>
/// System safety configuration for ShadowGuard
/// This is a duplicate of ShadowGuard's entity to avoid project dependency
/// </summary>
public class SystemSafetyConfig : BaseEntity
{
    // ElevenLabs Plan Information
    public string ElevenLabsPlanTier { get; set; } = "Free";
    public decimal MonthlyPrice { get; set; }
    public int MonthlyCredits { get; set; }
    public int CreditsUsed { get; set; }
    public int EstimatedMinutes { get; set; }
    public int MinutesUsed { get; set; }

    // Hard Limits
    public int AbsoluteMaxConcurrentConnections { get; set; } = 50;
    public int AbsoluteMaxVoiceSlots { get; set; } = 200;
    public int AbsoluteMaxConversationMinutes { get; set; } = 60;
    public int AbsoluteMaxDailyMinutesPerUser { get; set; } = 120;
    public int AbsoluteMaxDailyCredits { get; set; } = 100000;

    // Alert Thresholds
    public decimal CreditWarningThreshold { get; set; } = 0.8m;
    public decimal CreditDangerThreshold { get; set; } = 0.95m;

    // Feature Toggles
    public bool EnableHardLimits { get; set; } = true;
    public bool EnableAutoStop { get; set; } = true;
    public bool EnableDailyLimits { get; set; } = true;

    // Alert Configuration
    public string AlertEmail { get; set; } = string.Empty;
    public string AlertPhone { get; set; } = string.Empty;
    public string SlackWebhookUrl { get; set; } = string.Empty;

    // Billing Period
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public bool AutoRenew { get; set; } = true;

    // Maintenance Mode
    public bool IsMaintenanceMode { get; set; } = false;
    public string? MaintenanceMessage { get; set; }
    public DateTime? MaintenanceStartedAt { get; set; }
    public string? MaintenanceStartedBy { get; set; }

    // Computed Properties
    public int RemainingCredits => MonthlyCredits - CreditsUsed;
    public int RemainingMinutes => EstimatedMinutes - MinutesUsed;
    public bool IsActive { get; set; } = true;
}
