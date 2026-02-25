using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Billing;

public class Invoice : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? SubscriptionId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public DateTime IssuedDate { get; set; }
    public DateTime DueDate { get; set; }

    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }

    public string Currency { get; set; } = null!;
    public string Status { get; set; } = null!;

    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}

