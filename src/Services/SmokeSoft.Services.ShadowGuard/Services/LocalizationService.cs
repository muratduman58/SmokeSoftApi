using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmokeSoft.Services.ShadowGuard.Data;

namespace SmokeSoft.Services.ShadowGuard.Services;

public class LocalizationService : ILocalizationService
{
    private readonly ShadowGuardDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LocalizationService> _logger;
    private readonly IConfiguration _config;
    private const string CacheKeyPrefix = "localization:";
    private readonly int _cacheExpirationMinutes;

    public LocalizationService(
        ShadowGuardDbContext context,
        IMemoryCache cache,
        ILogger<LocalizationService> logger,
        IConfiguration config)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _config = config;
        _cacheExpirationMinutes = _config.GetValue<int>("Localization:CacheExpirationMinutes", 60);
    }

    public async Task<string> GetAsync(string key, string languageCode = "tr")
    {
        // Optimization: If language is English, return the key directly
        // ElevenLabs API already returns English values
        if (languageCode == "en")
        {
            return key.Split('.').Last(); // Return the last part of the key (e.g., "gender.male" -> "male")
        }

        var cacheKey = $"{CacheKeyPrefix}{languageCode}:{key}";
        
        // Check cache first
        if (_cache.TryGetValue(cacheKey, out string? cachedValue))
            return cachedValue!;

        // Query database
        var value = await _context.LocalizationStrings
            .Where(ls => ls.Key == key && ls.LanguageCode == languageCode)
            .Select(ls => ls.Value)
            .FirstOrDefaultAsync();

        // Fallback to English if not found
        if (value == null && languageCode != "en")
        {
            value = await GetAsync(key, "en");
        }

        // Fallback to key itself if still not found
        value ??= key;

        // Cache the result
        _cache.Set(cacheKey, value, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        return value;
    }

    public async Task<Dictionary<string, string>> GetCategoryAsync(string category, string languageCode = "tr")
    {
        var cacheKey = $"{CacheKeyPrefix}{languageCode}:category:{category}";
        
        // Check cache first
        if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? cached))
            return cached!;

        // Query database
        var translations = await _context.LocalizationStrings
            .Where(ls => ls.Category == category && ls.LanguageCode == languageCode)
            .ToDictionaryAsync(ls => ls.Key, ls => ls.Value);

        // Cache the result
        _cache.Set(cacheKey, translations, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        return translations;
    }

    public async Task RefreshCacheAsync()
    {
        _logger.LogInformation("Refreshing localization cache");
        
        // Note: In a production system, you might want to use a more sophisticated
        // cache invalidation strategy or use distributed cache with tags
        
        return;
    }
}
