using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.Admin.Data;
using SmokeSoft.Services.Admin.Data.Entities;
using SmokeSoft.Shared.Models;

namespace SmokeSoft.Services.Admin.Controllers;

[ApiController]
[Route("api/admin/system-config")]
[Authorize(Roles = "Admin")]
public class SystemConfigController : ControllerBase
{
    private readonly ShadowGuardDbContext _context;
    private readonly Services.ICacheInvalidationService _cacheInvalidation;
    
    public SystemConfigController(
        ShadowGuardDbContext context,
        Services.ICacheInvalidationService cacheInvalidation)
    {
        _context = context;
        _cacheInvalidation = cacheInvalidation;
    }
    
    // Helper method from BaseController
    protected IActionResult Success(object data, string? message = null)
    {
        return Ok(ApiResponse<object>.SuccessResult(data, message));
    }
    
    // Helper to get user email from JWT claims
    private string GetUserEmail()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "unknown";
    }
    
    // GET: Get current configuration
    [HttpGet]
    public async Task<IActionResult> GetConfig()
    {
        var config = await _context.SystemSafetyConfigs.FirstOrDefaultAsync(c => c.IsActive);
        if (config == null)
        {
            return NotFound("System configuration not found");
        }
        return Success(config);
    }
    
    // PUT: Update ElevenLabs plan
    [HttpPut("elevenlabs-plan")]
    public async Task<IActionResult> UpdateElevenLabsPlan([FromBody] UpdateElevenLabsPlanRequest request)
    {
        var config = await _context.SystemSafetyConfigs.FirstAsync(c => c.IsActive);
        
        config.ElevenLabsPlanTier = request.PlanTier;
        config.MonthlyPrice = request.MonthlyPrice;
        config.MonthlyCredits = request.MonthlyCredits;
        config.EstimatedMinutes = request.EstimatedMinutes;
        config.PeriodStartDate = DateTime.UtcNow;
        config.PeriodEndDate = DateTime.UtcNow.AddMonths(1);
        config.CreditsUsed = 0; // Reset for new period
        config.MinutesUsed = 0;
        
        await _context.SaveChangesAsync();
        // Cache invalidation not needed - direct DB access
        
        return Success(config, "ElevenLabs plan başarıyla güncellendi");
    }
    
    // PUT: Update hard limits
    [HttpPut("hard-limits")]
    public async Task<IActionResult> UpdateHardLimits([FromBody] UpdateHardLimitsRequest request)
    {
        var config = await _context.SystemSafetyConfigs.FirstAsync(c => c.IsActive);
        
        config.AbsoluteMaxConcurrentConnections = request.MaxConcurrentConnections;
        config.AbsoluteMaxVoiceSlots = request.MaxVoiceSlots;
        config.AbsoluteMaxConversationMinutes = request.MaxConversationMinutes;
        config.AbsoluteMaxDailyMinutesPerUser = request.MaxDailyMinutesPerUser;
        config.AbsoluteMaxDailyCredits = request.MaxDailyCredits;
        
        await _context.SaveChangesAsync();
        // Cache invalidation not needed - direct DB access
        
        return Success(new { }, "Hard limit'ler başarıyla güncellendi");
    }
    
    // PUT: Update alert settings
    [HttpPut("alerts")]
    public async Task<IActionResult> UpdateAlertSettings([FromBody] UpdateAlertSettingsRequest request)
    {
        var config = await _context.SystemSafetyConfigs.FirstAsync(c => c.IsActive);
        
        config.AlertEmail = request.Email;
        config.AlertPhone = request.Phone;
        config.SlackWebhookUrl = request.SlackWebhook;
        config.CreditWarningThreshold = request.WarningThreshold;
        config.CreditDangerThreshold = request.DangerThreshold;
        
        await _context.SaveChangesAsync();
        // Cache invalidation not needed - direct DB access
        
        return Success(new { }, "Alarm ayarları başarıyla güncellendi");
    }
    
    // PUT: Update feature toggles
    [HttpPut("features")]
    public async Task<IActionResult> UpdateFeatures([FromBody] UpdateFeaturesRequest request)
    {
        var config = await _context.SystemSafetyConfigs.FirstAsync(c => c.IsActive);
        
        config.EnableHardLimits = request.EnableHardLimits;
        config.EnableAutoStop = request.EnableAutoStop;
        config.EnableDailyLimits = request.EnableDailyLimits;
        
        await _context.SaveChangesAsync();
        // Cache invalidation not needed - direct DB access
        
        return Success(new { }, "Özellikler başarıyla güncellendi");
    }
    
    // GET: Real-time dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var config = await _context.SystemSafetyConfigs.FirstOrDefaultAsync(c => c.IsActive);
        if (config == null)
        {
            return NotFound("System configuration not found");
        }
        
        var now = DateTime.UtcNow;
        
        // Today's usage - Commented out due to missing entities in Admin API
        // var todayCredits = await _context.CreditUsageLogs
        //     .Where(l => l.UsedAt >= now.Date)
        //     .SumAsync(l => l.CreditsUsed);
        
        // var todayMinutes = await _context.Conversations
        //     .Where(c => c.IsAICall && c.CallStartedAt >= now.Date)
        //     .SumAsync(c => c.MinutesUsed);
        
        // Active resources - Commented out due to missing entities
        // var activeConnections = await _context.ConversationSessions
        //     .CountAsync(s => s.EndedAt == null);
        
        // var activeSlots = await _context.VoiceSlots
        //     .CountAsync(s => s.IsActive);
        
        var todayCredits = 0;
        var todayMinutes = 0;
        var activeConnections = 0;
        var activeSlots = 0;
        
        // Status
        var creditPercentage = (config.CreditsUsed * 100.0m) / (config.MonthlyCredits > 0 ? config.MonthlyCredits : 1);
        var status = creditPercentage >= config.CreditDangerThreshold * 100 ? "DANGER" 
                   : creditPercentage >= config.CreditWarningThreshold * 100 ? "WARNING" 
                   : "OK";
        
        return Success(new
        {
            status,
            plan = new
            {
                tier = config.ElevenLabsPlanTier,
                monthlyPrice = config.MonthlyPrice,
                periodEnd = config.PeriodEndDate
            },
            credits = new
            {
                total = config.MonthlyCredits,
                used = config.CreditsUsed,
                remaining = config.RemainingCredits,
                percentage = creditPercentage,
                todayUsage = todayCredits
            },
            minutes = new
            {
                estimated = config.EstimatedMinutes,
                used = config.MinutesUsed,
                remaining = config.RemainingMinutes,
                todayUsage = todayMinutes
            },
            active = new
            {
                connections = activeConnections,
                slots = activeSlots,
                maxConnections = config.AbsoluteMaxConcurrentConnections,
                maxSlots = config.AbsoluteMaxVoiceSlots
            },
            limits = new
            {
                hardLimitsEnabled = config.EnableHardLimits,
                autoStopEnabled = config.EnableAutoStop,
                dailyLimitsEnabled = config.EnableDailyLimits
            }
        });
    }
    
    // PUT: Toggle maintenance mode
    [HttpPut("maintenance")]
    public async Task<IActionResult> ToggleMaintenanceMode([FromBody] MaintenanceRequest request)
    {
        var config = await _context.SystemSafetyConfigs.FirstAsync(c => c.IsActive);
        var adminEmail = GetUserEmail();
        
        config.IsMaintenanceMode = request.IsEnabled;
        
        if (request.IsEnabled)
        {
            config.MaintenanceMessage = request.Message ?? "Sistem bakımda. Lütfen daha sonra tekrar deneyin.";
            config.MaintenanceStartedAt = DateTime.UtcNow;
            config.MaintenanceStartedBy = adminEmail;
        }
        else
        {
            config.MaintenanceMessage = null;
            config.MaintenanceStartedAt = null;
            config.MaintenanceStartedBy = null;
        }
        
        await _context.SaveChangesAsync();
        
        // Invalidate cache so ShadowGuard API picks up the change immediately
        await _cacheInvalidation.InvalidateSystemConfigCacheAsync();
        
        return Success(new
        {
            isMaintenanceMode = config.IsMaintenanceMode,
            message = config.MaintenanceMessage,
            startedAt = config.MaintenanceStartedAt,
            startedBy = config.MaintenanceStartedBy
        }, request.IsEnabled ? "Bakım modu aktif edildi" : "Bakım modu devre dışı bırakıldı");
    }
}

// Request DTOs
public record UpdateElevenLabsPlanRequest(
    string PlanTier,
    decimal MonthlyPrice,
    int MonthlyCredits,
    int EstimatedMinutes);

public record UpdateHardLimitsRequest(
    int MaxConcurrentConnections,
    int MaxVoiceSlots,
    int MaxConversationMinutes,
    int MaxDailyMinutesPerUser,
    int MaxDailyCredits);

public record UpdateAlertSettingsRequest(
    string Email,
    string Phone,
    string SlackWebhook,
    decimal WarningThreshold,
    decimal DangerThreshold);

public record UpdateFeaturesRequest(
    bool EnableHardLimits,
    bool EnableAutoStop,
    bool EnableDailyLimits);

public record EnableMaintenanceRequest(string? Message);

public record MaintenanceRequest(bool IsEnabled, string? Message = null);

