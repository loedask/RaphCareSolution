using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Insurance;

public class CoverageValidation : BaseEntity
{
    public Guid SubscriptionId { get; set; }

    public Guid? VisitId { get; set; }
    public Guid? TeleSessionId { get; set; }
    public Guid? DeviceReadingId { get; set; }

    public decimal ApprovedAmount { get; set; }
    public decimal PatientResponsibilityAmount { get; set; }

    public string ValidationStatus { get; set; } = null!;
    public DateTime ValidatedAt { get; set; }

    public Subscription Subscription { get; set; } = null!;
}

