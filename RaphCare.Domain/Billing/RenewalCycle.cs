using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Billing;

public class RenewalCycle : BaseEntity
{
    public Guid SubscriptionId { get; set; }

    public DateTime NextBillingDate { get; set; }
    public decimal BillingAmount { get; set; }
    public string Currency { get; set; } = null!;
    public bool AutoRenew { get; set; }
    public bool IsActive { get; set; }
}

