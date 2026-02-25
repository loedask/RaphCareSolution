using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Insurance;

public class PlanBenefit : BaseEntity
{
    public Guid InsurancePlanId { get; set; }

    public string BenefitName { get; set; } = null!;
    public string Description { get; set; } = null!;

    public decimal? CoverageLimitAmount { get; set; }
    public int? CoverageLimitCount { get; set; }
    public bool IsUnlimited { get; set; }
    public bool IsActive { get; set; }

    public InsurancePlan InsurancePlan { get; set; } = null!;
}

