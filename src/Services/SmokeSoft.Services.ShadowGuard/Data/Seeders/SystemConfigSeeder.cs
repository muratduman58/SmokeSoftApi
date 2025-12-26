using SmokeSoft.Services.ShadowGuard.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Seeders;

public static class SystemConfigSeeder
{
    public static async Task SeedAsync(ShadowGuardDbContext context)
    {
        // Check if SystemSafetyConfig already exists
        if (context.SystemSafetyConfigs.Any(c => c.IsActive))
        {
            return; // Already seeded
        }

        // Create default system configuration
        var defaultConfig = new SystemSafetyConfig
        {
            // ElevenLabs Plan Configuration
            ElevenLabsPlanTier = "Free",
            MonthlyPrice = 0,
            MonthlyCredits = 10000,
            CreditsUsed = 0,
            EstimatedMinutes = 20,
            MinutesUsed = 0,

            // Hard Limits
            AbsoluteMaxConcurrentConnections = 50,
            AbsoluteMaxVoiceSlots = 200,
            AbsoluteMaxConversationMinutes = 60,
            AbsoluteMaxDailyMinutesPerUser = 120,
            AbsoluteMaxDailyCredits = 100000,

            // Alert Thresholds
            CreditWarningThreshold = 0.8m, // 80%
            CreditDangerThreshold = 0.95m, // 95%

            // Feature Toggles
            EnableHardLimits = true,
            EnableAutoStop = true,
            EnableDailyLimits = true,

            // Alert Configuration
            AlertEmail = string.Empty,
            AlertPhone = string.Empty,
            SlackWebhookUrl = string.Empty,

            // Billing Period
            PeriodStartDate = DateTime.UtcNow,
            PeriodEndDate = DateTime.UtcNow.AddMonths(1),
            AutoRenew = true,

            // Maintenance Mode (default: disabled)
            IsMaintenanceMode = false,
            MaintenanceMessage = null,
            MaintenanceStartedAt = null,
            MaintenanceStartedBy = null,

            // Status
            IsActive = true
        };

        context.SystemSafetyConfigs.Add(defaultConfig);
        await context.SaveChangesAsync();
    }
}
