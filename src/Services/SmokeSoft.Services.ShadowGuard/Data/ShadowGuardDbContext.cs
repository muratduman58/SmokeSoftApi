using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.ShadowGuard.Data.Entities;
using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.ShadowGuard.Data;

public class ShadowGuardDbContext : DbContext
{
    public ShadowGuardDbContext(DbContextOptions<ShadowGuardDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AIIdentity> AIIdentities { get; set; }
    public DbSet<VoiceRecording> VoiceRecordings { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<UserOAuthProvider> UserOAuthProviders { get; set; }
    public DbSet<ScreenCustomization> ScreenCustomizations { get; set; }
    public DbSet<PurchaseVerification> PurchaseVerifications { get; set; }
    
    // ElevenLabs & Safety Management
    public DbSet<SystemSafetyConfig> SystemSafetyConfigs { get; set; }
    public DbSet<VoiceSample> VoiceSamples { get; set; }
    public DbSet<VoiceSlot> VoiceSlots { get; set; }
    public DbSet<ConversationSession> ConversationSessions { get; set; }
    public DbSet<CreditUsageLog> CreditUsageLogs { get; set; }
    public DbSet<LocalizationString> LocalizationStrings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set schema
        modelBuilder.HasDefaultSchema("shadowguard");

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasMany(e => e.RefreshTokens)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.AIIdentities)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Conversations)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Devices)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.OAuthProviders)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UserProfile)
                .WithOne(e => e.User)
                .HasForeignKey<UserProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.Token).IsRequired();
        });

        // AIIdentity configuration
        modelBuilder.Entity<AIIdentity>(entity =>
        {
            entity.ToTable("ai_identities");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GreetingStyle).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Catchphrases).HasMaxLength(2000);
            entity.Property(e => e.ExpertiseArea).IsRequired().HasMaxLength(200);

            entity.HasMany(e => e.VoiceRecordings)
                .WithOne(e => e.AIIdentity)
                .HasForeignKey(e => e.AIIdentityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Conversations)
                .WithOne(e => e.AIIdentity)
                .HasForeignKey(e => e.AIIdentityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // VoiceRecording configuration
        modelBuilder.Entity<VoiceRecording>(entity =>
        {
            entity.ToTable("voice_recordings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
        });

        // UserProfile configuration
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Settings).HasMaxLength(4000);

            entity.HasOne(e => e.PreferredAIIdentity)
                .WithMany()
                .HasForeignKey(e => e.PreferredAIIdentityId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Conversation configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("conversations");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.StartedAt });

            entity.HasMany(e => e.Messages)
                .WithOne(e => e.Conversation)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(5000);
            entity.HasIndex(e => new { e.ConversationId, e.Timestamp });
        });

        // Device configuration
        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("devices");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DeviceId).IsUnique();
            entity.Property(e => e.DeviceId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.DeviceName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DeviceModel).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Platform).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PlatformVersion).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AppVersion).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FcmToken).HasMaxLength(500);
            entity.HasIndex(e => e.UserId);
        });

        // UserOAuthProvider configuration
        modelBuilder.Entity<UserOAuthProvider>(entity =>
        {
            entity.ToTable("user_oauth_providers");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Provider, e.ProviderUserId }).IsUnique();
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProviderUserId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            entity.HasIndex(e => e.UserId);
        });

        // ScreenCustomization configuration
        modelBuilder.Entity<ScreenCustomization>(entity =>
        {
            entity.ToTable("screen_customizations");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DeviceId, e.ScreenType });
            entity.Property(e => e.DeviceId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ScreenType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DeviceName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DeviceModel).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Platform).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PlatformVersion).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AppVersion).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.UserId);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // LocalizationString configuration
        modelBuilder.Entity<LocalizationString>(entity =>
        {
            entity.ToTable("localization_strings");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Key, e.LanguageCode }).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(255);
            entity.Property(e => e.LanguageCode).IsRequired().HasMaxLength(5);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
        });

        // Configure BaseEntity properties for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("CreatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Create properly typed query filter for soft delete
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var propertyMethod = typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(bool));
                var isDeletedProperty = System.Linq.Expressions.Expression.Call(
                    propertyMethod,
                    parameter,
                    System.Linq.Expressions.Expression.Constant("IsDeleted"));
                var notDeleted = System.Linq.Expressions.Expression.Not(isDeletedProperty);
                var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
