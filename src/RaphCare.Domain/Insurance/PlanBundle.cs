using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Insurance;

public class PlanBundle : BaseEntity
{
    public Guid InsurancePlanId { get; set; }

    public string BundleName { get; set; } = null!;
    public string Description { get; set; } = null!;

    public bool IncludesTelemedicine { get; set; }
    public bool IncludesDeviceMonitoring { get; set; }
    public bool IncludesMentalHealth { get; set; }

    public InsurancePlan InsurancePlan { get; set; } = null!;
}

