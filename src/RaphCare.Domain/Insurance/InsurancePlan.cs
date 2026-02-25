using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Insurance;

public class InsurancePlan : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }

    public string Name { get; set; } = null!;
    public string PlanCode { get; set; } = null!;
    public string Description { get; set; } = null!;

    public decimal MonthlyPremium { get; set; }
    public decimal? AnnualPremium { get; set; }
    public string Currency { get; set; } = null!;

    public bool IsActive { get; set; }
    public bool IsMicroInsurance { get; set; }

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public ICollection<PlanBenefit> Benefits { get; set; } = new List<PlanBenefit>();
    public ICollection<CoverageRule> CoverageRules { get; set; } = new List<CoverageRule>();
    public ICollection<EligibilityRule> EligibilityRules { get; set; } = new List<EligibilityRule>();
    public ICollection<PlanBundle> Bundles { get; set; } = new List<PlanBundle>();
    public ICollection<CommissionStructure> CommissionStructures { get; set; } = new List<CommissionStructure>();
}

