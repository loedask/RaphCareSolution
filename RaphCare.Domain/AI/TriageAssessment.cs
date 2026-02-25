using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.AI;

public class TriageAssessment : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }

    public string Symptoms { get; set; } = null!;
    public string TriageLevel { get; set; } = null!;
    public string Recommendation { get; set; } = null!;
    public bool IsReviewedByProvider { get; set; }
}

