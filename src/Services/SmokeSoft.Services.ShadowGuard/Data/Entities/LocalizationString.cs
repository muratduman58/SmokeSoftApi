using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Entities;

public class LocalizationString : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Category { get; set; }
}
