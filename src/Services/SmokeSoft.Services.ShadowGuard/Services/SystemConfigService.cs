using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Services.ShadowGuard.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Services;

public interface ISystemConfigService
{
    Task<SystemSafetyConfig> GetConfigAsync();
    Task InvalidateCacheAsync();
}

public class SystemConfigService : ISystemConfigService
{
    private readonly ShadowGuardDbContext _context;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY = "system_safety_config";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);
    
    public SystemConfigService(ShadowGuardDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }
    
    public async Task<SystemSafetyConfig> GetConfigAsync()
    {
        // Try cache first
        if (_cache.TryGetValue(CACHE_KEY, out SystemSafetyConfig? cached))
        {
            return cached!;
        }
        
        // Load from database
        var config = await _context.SystemSafetyConfigs
            .FirstOrDefaultAsync(c => c.IsActive)
            ?? throw new InvalidOperationException("System safety configuration not found. Please configure the system.");
        
        // Cache it
        _cache.Set(CACHE_KEY, config, CacheDuration);
        
        return config;
    }
    
    public async Task InvalidateCacheAsync()
    {
        _cache.Remove(CACHE_KEY);
        await Task.CompletedTask;
    }
}
