using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.AI;

public class RiskScore : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }

    public decimal Score { get; set; }
    public string RiskCategory { get; set; } = null!;
    public DateTime CalculatedAt { get; set; }
}

