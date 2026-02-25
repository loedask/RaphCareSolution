using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Insurance;

public class EligibilityRule : BaseEntity
{
    public Guid InsurancePlanId { get; set; }

    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public string CountryRestriction { get; set; } = null!;
    public bool RequiresMedicalAssessment { get; set; }
    public bool IsActive { get; set; }

    public InsurancePlan InsurancePlan { get; set; } = null!;
}

