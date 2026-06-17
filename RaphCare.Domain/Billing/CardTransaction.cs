using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Billing;

public class CardTransaction : BaseEntity
{
    public Guid PaymentTransactionId { get; set; }

    public string CardBrand { get; set; } = null!;
    public string MaskedCardNumber { get; set; } = null!;
    public string AuthorizationCode { get; set; } = null!;
    public string ProcessorResponse { get; set; } = null!;

    public DateTime AuthorizedAt { get; set; }
    public bool Is3DSecure { get; set; }

    public PaymentTransaction PaymentTransaction { get; set; } = null!;
}

