using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Billing;

public class MobileMoneyTransaction : BaseEntity
{
    public Guid PaymentTransactionId { get; set; }

    public string MobileNumber { get; set; } = null!;
    public string NetworkProvider { get; set; } = null!;
    public string TransactionReference { get; set; } = null!;

    public DateTime InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string Status { get; set; } = null!;

    public PaymentTransaction PaymentTransaction { get; set; } = null!;
}

