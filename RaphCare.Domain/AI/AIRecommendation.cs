using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.AI;

public class AIRecommendation : BaseEntity
{
    public Guid PatientId { get; set; }

    public string Context { get; set; } = null!;
    public string RecommendationText { get; set; } = null!;
    public bool IsAccepted { get; set; }
}

