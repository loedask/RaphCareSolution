using RaphCare.Domain.Common;

namespace RaphCare.Domain.AI;

public class WellnessInsight : BaseEntity
{
    public Guid PatientId { get; set; }

    public string InsightType { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }
    public bool IsAIGenerated { get; set; }
}

