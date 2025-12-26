using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Services.ShadowGuard.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data.Seeders;

public static class LocalizationSeeder
{
    public static async Task SeedAsync(ShadowGuardDbContext context)
    {
        if (context.LocalizationStrings.Any())
        {
            return; // Already seeded
        }

        var translations = new List<LocalizationString>
        {
            // Gender
            new() { Id = Guid.NewGuid(), Key = "gender.male", LanguageCode = "tr", Value = "Erkek", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "gender.male", LanguageCode = "en", Value = "Male", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "gender.female", LanguageCode = "tr", Value = "Kadın", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "gender.female", LanguageCode = "en", Value = "Female", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "gender.neutral", LanguageCode = "tr", Value = "Nötr", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "gender.neutral", LanguageCode = "en", Value = "Neutral", Category = "voice_metadata" },
            
            // Accent
            new() { Id = Guid.NewGuid(), Key = "accent.american", LanguageCode = "tr", Value = "Amerikan", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.american", LanguageCode = "en", Value = "American", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.british", LanguageCode = "tr", Value = "İngiliz", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.british", LanguageCode = "en", Value = "British", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.australian", LanguageCode = "tr", Value = "Avustralya", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.australian", LanguageCode = "en", Value = "Australian", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.indian", LanguageCode = "tr", Value = "Hint", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.indian", LanguageCode = "en", Value = "Indian", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.african", LanguageCode = "tr", Value = "Afrika", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.african", LanguageCode = "en", Value = "African", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.irish", LanguageCode = "tr", Value = "İrlanda", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.irish", LanguageCode = "en", Value = "Irish", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.scottish", LanguageCode = "tr", Value = "İskoç", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "accent.scottish", LanguageCode = "en", Value = "Scottish", Category = "voice_metadata" },
            
            // Age
            new() { Id = Guid.NewGuid(), Key = "age.young", LanguageCode = "tr", Value = "Genç", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "age.young", LanguageCode = "en", Value = "Young", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "age.middle_aged", LanguageCode = "tr", Value = "Orta Yaş", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "age.middle_aged", LanguageCode = "en", Value = "Middle Aged", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "age.middle aged", LanguageCode = "tr", Value = "Orta Yaş", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "age.middle aged", LanguageCode = "en", Value = "Middle Aged", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "age.old", LanguageCode = "tr", Value = "Yaşlı", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "age.old", LanguageCode = "en", Value = "Old", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "age.elderly", LanguageCode = "tr", Value = "Yaşlı", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "age.elderly", LanguageCode = "en", Value = "Elderly", Category = "voice_metadata" },
            
            // Use Case
            new() { Id = Guid.NewGuid(), Key = "usecase.narration", LanguageCode = "tr", Value = "Anlatım", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.narration", LanguageCode = "en", Value = "Narration", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.news", LanguageCode = "tr", Value = "Haber", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.news", LanguageCode = "en", Value = "News", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.audiobook", LanguageCode = "tr", Value = "Sesli Kitap", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.audiobook", LanguageCode = "en", Value = "Audiobook", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.conversational", LanguageCode = "tr", Value = "Konuşma", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.conversational", LanguageCode = "en", Value = "Conversational", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.characters", LanguageCode = "tr", Value = "Karakterler", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.characters", LanguageCode = "en", Value = "Characters", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.animation", LanguageCode = "tr", Value = "Animasyon", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.animation", LanguageCode = "en", Value = "Animation", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.video games", LanguageCode = "tr", Value = "Video Oyunları", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.video games", LanguageCode = "en", Value = "Video Games", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.meditation", LanguageCode = "tr", Value = "Meditasyon", Category = "voice_metadata" },
            new() { Id = Guid.NewGuid(), Key = "usecase.meditation", LanguageCode = "en", Value = "Meditation", Category = "voice_metadata" },
        };

        context.LocalizationStrings.AddRange(translations);
        await context.SaveChangesAsync();
    }
}
