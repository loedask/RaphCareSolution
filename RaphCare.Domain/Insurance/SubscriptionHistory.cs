using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Insurance;

public class SubscriptionHistory : BaseEntity
{
    public Guid SubscriptionId { get; set; }

    public string PreviousStatus { get; set; } = null!;
    public string NewStatus { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = null!;

    public Subscription Subscription { get; set; } = null!;
}

