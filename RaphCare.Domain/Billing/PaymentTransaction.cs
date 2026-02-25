using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Billing;

public class PaymentTransaction : BaseEntity, IAggregateRoot
{
    public Guid ClinicId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? PaymentMethodId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;
    public string PaymentChannel { get; set; } = null!;
    public string ExternalTransactionReference { get; set; } = null!;

    public DateTime ProcessedAt { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public PaymentMethod? PaymentMethod { get; set; }
    public MobileMoneyTransaction? MobileMoneyTransaction { get; set; }
    public CardTransaction? CardTransaction { get; set; }
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}

