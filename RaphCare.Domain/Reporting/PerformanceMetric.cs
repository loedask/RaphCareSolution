using RaphCare.Domain.Common;

namespace RaphCare.Domain.Reporting;

/// <summary>
/// Single performance metric recorded for a clinic.
/// </summary>
public class PerformanceMetric : BaseEntity
{
    public Guid ClinicId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime RecordedAt { get; set; }
}
