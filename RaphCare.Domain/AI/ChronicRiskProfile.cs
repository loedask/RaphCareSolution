using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.AI;

public class ChronicRiskProfile : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }

    public string Condition { get; set; } = null!;
    public decimal RiskScore { get; set; }
    public string RiskLevel { get; set; } = null!;
}

