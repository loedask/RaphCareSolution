using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Precomputed analytics snapshot for AI and reporting.
/// </summary>
public class TrendSummary : BaseEntity
{
    public Guid PatientId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public decimal AverageValue { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
