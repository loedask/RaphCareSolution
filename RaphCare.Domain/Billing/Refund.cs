using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Billing;

public class Refund : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Guid PaymentTransactionId { get; set; }

    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime RefundedAt { get; set; }
    public string Status { get; set; } = null!;

    public Invoice Invoice { get; set; } = null!;
    public PaymentTransaction PaymentTransaction { get; set; } = null!;
}

