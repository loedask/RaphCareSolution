using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Insurance;

public class CommissionStructure : BaseEntity
{
    public Guid InsurancePlanId { get; set; }

    public decimal ProviderCommissionPercentage { get; set; }
    public decimal ClinicCommissionPercentage { get; set; }
    public bool IsActive { get; set; }

    public InsurancePlan InsurancePlan { get; set; } = null!;
}

