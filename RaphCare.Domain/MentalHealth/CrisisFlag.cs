using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.MentalHealth;

public class CrisisFlag : BaseEntity
{
    public Guid TherapySessionId { get; set; }
    public Guid PatientId { get; set; }

    public string RiskLevel { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime FlaggedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public TherapySession TherapySession { get; set; } = null!;
}

