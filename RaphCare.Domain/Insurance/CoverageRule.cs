using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Insurance;

public class CoverageRule : BaseEntity
{
    public Guid InsurancePlanId { get; set; }

    public string ServiceType { get; set; } = null!;
    public decimal CoveragePercentage { get; set; }
    public decimal? MaxCoverageAmount { get; set; }
    public bool RequiresPreAuthorization { get; set; }
    public bool IsActive { get; set; }

    public InsurancePlan InsurancePlan { get; set; } = null!;
}

