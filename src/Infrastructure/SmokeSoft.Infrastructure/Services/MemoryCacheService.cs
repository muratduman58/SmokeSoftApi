using Microsoft.Extensions.Caching.Memory;
using SmokeSoft.Infrastructure.Caching;

namespace SmokeSoft.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For interface compatibility
        return _cache.TryGetValue(key, out T? value) ? value : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For interface compatibility
        
        var options = new MemoryCacheEntryOptions();
        
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }
        else
        {
            // Default expiration: 1 hour
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        }

        _cache.Set(key, value, options);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For interface compatibility
        _cache.Remove(key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For interface compatibility
        return _cache.TryGetValue(key, out _);
    }
}
