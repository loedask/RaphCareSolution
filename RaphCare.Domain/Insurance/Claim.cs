using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Insurance;

public class Claim : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }

    public Guid SubscriptionId { get; set; }
    public Guid? VisitId { get; set; }
    public Guid? TeleSessionId { get; set; }

    public decimal TotalClaimAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal PatientResponsibilityAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime SubmittedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public Subscription Subscription { get; set; } = null!;
    public ICollection<ClaimLineItem> LineItems { get; set; } = new List<ClaimLineItem>();
    public ICollection<ReimbursementRecord> Reimbursements { get; set; } = new List<ReimbursementRecord>();
}

