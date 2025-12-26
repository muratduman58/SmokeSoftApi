using SmokeSoft.Shared.Common;

namespace SmokeSoft.Services.ShadowGuard.Services;

public interface ILocalizationService
{
    Task<string> GetAsync(string key, string languageCode = "tr");
    Task<Dictionary<string, string>> GetCategoryAsync(string category, string languageCode = "tr");
    Task RefreshCacheAsync();
}
