using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.Admin.Data.Entities;

namespace SmokeSoft.Services.Admin.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }
    
    // Admin-specific tables
    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<SystemMetric> SystemMetrics { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("admin");
        
        // AdminUser configuration
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.ToTable("admin_users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });
        
        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AdminUserId, e.Timestamp });
            entity.HasIndex(e => e.Action);
        });
        
        // SystemMetric configuration
        modelBuilder.Entity<SystemMetric>(entity =>
        {
            entity.ToTable("system_metrics");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ServiceName, e.Timestamp });
            entity.HasIndex(e => e.MetricType);
        });
    }
}
