using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.Admin.Data.Entities;

namespace SmokeSoft.Services.Admin.Data;

/// <summary>
/// DbContext for accessing ShadowGuard database from Admin API
/// Read-only access to manage system configuration
/// </summary>
public class ShadowGuardDbContext : DbContext
{
    public ShadowGuardDbContext(DbContextOptions<ShadowGuardDbContext> options) : base(options)
    {
    }

    public DbSet<SystemSafetyConfig> SystemSafetyConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schema
        modelBuilder.HasDefaultSchema("shadowguard");

        // SystemSafetyConfig configuration
        modelBuilder.Entity<SystemSafetyConfig>(entity =>
        {
            entity.ToTable("SystemSafetyConfigs");
            entity.HasKey(e => e.Id);
            
            // Computed properties are ignored
            entity.Ignore(e => e.RemainingCredits);
            entity.Ignore(e => e.RemainingMinutes);
        });
    }
}
