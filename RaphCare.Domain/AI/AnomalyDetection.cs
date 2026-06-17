using RaphCare.Domain.Common;

namespace RaphCare.Domain.AI;

public class AnomalyDetection : BaseEntity
{
    public Guid PatientId { get; set; }

    public string MetricName { get; set; } = null!;
    public decimal ObservedValue { get; set; }
    public string Severity { get; set; } = null!;
    public DateTime DetectedAt { get; set; }
}

