using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.Admin.Data.Entities;

public class SystemMetric : BaseEntity
{
    public string ServiceName { get; set; } = string.Empty; // ShadowGuard, Gateway, Admin
    public string MetricType { get; set; } = string.Empty; // CPU, Memory, RequestCount, ErrorCount
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty; // %, MB, count
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = string.Empty; // JSON
}
