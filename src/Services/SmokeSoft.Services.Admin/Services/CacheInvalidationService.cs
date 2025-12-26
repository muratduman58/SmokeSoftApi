using System.Net.Http;

namespace SmokeSoft.Services.Admin.Services;

/// <summary>
/// Service to invalidate ShadowGuard API cache via HTTP
/// Since Admin API and ShadowGuard API run in separate processes,
/// we need to call ShadowGuard's cache invalidation endpoint
/// </summary>
public interface ICacheInvalidationService
{
    Task InvalidateSystemConfigCacheAsync();
}

public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CacheInvalidationService> _logger;

    public CacheInvalidationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<CacheInvalidationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvalidateSystemConfigCacheAsync()
    {
        try
        {
            // Get ShadowGuard API URL from configuration
            var shadowGuardUrl = _configuration["ShadowGuardApiUrl"] ?? "http://localhost:5076";
            
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{shadowGuardUrl}/api/shadowguard/System/cache/invalidate", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully invalidated ShadowGuard API cache");
            }
            else
            {
                _logger.LogWarning("Failed to invalidate ShadowGuard API cache. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating ShadowGuard API cache");
            // Don't throw - cache invalidation failure shouldn't break the maintenance mode toggle
        }
    }
}
