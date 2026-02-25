using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Insurance;

public class ReimbursementRecord : BaseEntity
{
    public Guid ClaimId { get; set; }

    public decimal AmountPaid { get; set; }
    public DateTime PaidAt { get; set; }
    public string PaymentReference { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;

    public Claim Claim { get; set; } = null!;
}

