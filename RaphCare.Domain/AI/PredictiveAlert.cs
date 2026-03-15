using RaphCare.Domain.Common;

namespace RaphCare.Domain.AI;

public class PredictiveAlert : BaseEntity
{
    public Guid PatientId { get; set; }

    public string AlertType { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime TriggeredAt { get; set; }
    public bool IsResolved { get; set; }
}

