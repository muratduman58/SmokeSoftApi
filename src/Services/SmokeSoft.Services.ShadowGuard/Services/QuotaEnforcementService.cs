using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Services.ShadowGuard.Data.Entities;
using SmokeSoft.Shared.Common;

namespace SmokeSoft.Services.ShadowGuard.Services;

public interface IQuotaEnforcementService
{
    Task<Result> PreFlightCheckAsync(Guid userId);
    Task<Result> CheckConcurrencyLimitAsync();
    Task<Result> CheckDailyUserLimitAsync(Guid userId);
    Task<Result> CheckDailySystemLimitAsync();
    Task<bool> IsSystemHealthyAsync();
}

public class QuotaEnforcementService : IQuotaEnforcementService
{
    private readonly ShadowGuardDbContext _context;
    private readonly ISystemConfigService _configService;
    
    public QuotaEnforcementService(
        ShadowGuardDbContext context,
        ISystemConfigService configService)
    {
        _context = context;
        _configService = configService;
    }
    
    // Multi-layer pre-flight check
    public async Task<Result> PreFlightCheckAsync(Guid userId)
    {
        var checks = new[]
        {
            await CheckConcurrencyLimitAsync(),
            await CheckDailyUserLimitAsync(userId),
            await CheckDailySystemLimitAsync(),
            await CheckCreditAvailabilityAsync()
        };
        
        var failed = checks.FirstOrDefault(c => !c.IsSuccess);
        return failed ?? Result.Success();
    }
    
    // Check concurrent connections
    public async Task<Result> CheckConcurrencyLimitAsync()
    {
        var config = await _configService.GetConfigAsync();
        
        if (!config.EnableHardLimits)
            return Result.Success();
        
        var activeConnections = await _context.ConversationSessions
            .CountAsync(s => s.EndedAt == null);
        
        if (activeConnections >= config.AbsoluteMaxConcurrentConnections)
        {
            return Result.Failure(
                $"Maximum concurrent connections ({config.AbsoluteMaxConcurrentConnections}) reached. Active: {activeConnections}",
                "CONCURRENCY_LIMIT_EXCEEDED");
        }
        
        return Result.Success();
    }
    
    // Check daily user limit
    public async Task<Result> CheckDailyUserLimitAsync(Guid userId)
    {
        var config = await _configService.GetConfigAsync();
        
        if (!config.EnableDailyLimits)
            return Result.Success();
        
        var today = DateTime.UtcNow.Date;
        var todayMinutes = await _context.Conversations
            .Where(c => c.UserId == userId 
                     && c.IsAICall 
                     && c.CallStartedAt >= today)
            .SumAsync(c => c.MinutesUsed);
        
        if (todayMinutes >= config.AbsoluteMaxDailyMinutesPerUser)
        {
            return Result.Failure(
                $"Daily user limit ({config.AbsoluteMaxDailyMinutesPerUser} minutes) exceeded. Used: {todayMinutes}",
                "DAILY_USER_LIMIT_EXCEEDED");
        }
        
        return Result.Success();
    }
    
    // Check daily system limit
    public async Task<Result> CheckDailySystemLimitAsync()
    {
        var config = await _configService.GetConfigAsync();
        
        if (!config.EnableDailyLimits)
            return Result.Success();
        
        var today = DateTime.UtcNow.Date;
        var todayCredits = await _context.CreditUsageLogs
            .Where(l => l.UsedAt >= today)
            .SumAsync(l => l.CreditsUsed);
        
        if (todayCredits >= config.AbsoluteMaxDailyCredits)
        {
            return Result.Failure(
                $"Daily system credit limit exceeded. Used: {todayCredits}/{config.AbsoluteMaxDailyCredits}",
                "DAILY_SYSTEM_LIMIT_EXCEEDED");
        }
        
        return Result.Success();
    }
    
    // Check credit availability
    private async Task<Result> CheckCreditAvailabilityAsync()
    {
        var config = await _configService.GetConfigAsync();
        
        if (config.RemainingCredits <= 0)
        {
            return Result.Failure(
                $"Monthly credit limit exceeded. Plan: {config.ElevenLabsPlanTier}",
                "MONTHLY_CREDIT_LIMIT_EXCEEDED");
        }
        
        return Result.Success();
    }
    
    // Circuit breaker: Is system healthy?
    public async Task<bool> IsSystemHealthyAsync()
    {
        var config = await _configService.GetConfigAsync();
        
        // 95% credit usage → Circuit breaker
        if (config.CreditsUsed >= config.MonthlyCredits * config.CreditDangerThreshold)
        {
            return false;
        }
        
        return true;
    }
}
