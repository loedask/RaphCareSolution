using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Insurance;

public class Subscription : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }

    public Guid PatientId { get; set; }
    public Guid InsurancePlanId { get; set; }

    public string MembershipNumber { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string Status { get; set; } = null!;
    public bool AutoRenew { get; set; }
    public decimal CurrentPremium { get; set; }

    public InsurancePlan InsurancePlan { get; set; } = null!;
    public ICollection<SubscriptionHistory> History { get; set; } = new List<SubscriptionHistory>();
    public ICollection<CoverageValidation> CoverageValidations { get; set; } = new List<CoverageValidation>();
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}

