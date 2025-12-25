using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.Admin.Data.Entities;

public class AuditLog : BaseEntity
{
    public Guid AdminUserId { get; set; }
    public string Action { get; set; } = string.Empty; // UpdatePlan, UpdateLimits, etc.
    public string ServiceName { get; set; } = string.Empty; // ShadowGuard, FutureService
    public string Details { get; set; } = string.Empty; // JSON
    public string IpAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public AdminUser AdminUser { get; set; } = null!;
}
