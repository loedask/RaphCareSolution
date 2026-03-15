using RaphCare.Domain.Common;

namespace RaphCare.Domain.AI;

public class TrendAnalysis : BaseEntity
{
    public Guid PatientId { get; set; }

    public string MetricName { get; set; } = null!;
    public decimal AverageValue { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

