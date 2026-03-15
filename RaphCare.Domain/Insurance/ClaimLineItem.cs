using RaphCare.Domain.Common;

namespace RaphCare.Domain.Insurance;

public class ClaimLineItem : BaseEntity
{
    public Guid ClaimId { get; set; }

    public string ServiceCode { get; set; } = null!;
    public string Description { get; set; } = null!;

    public decimal BilledAmount { get; set; }
    public decimal ApprovedAmount { get; set; }

    public string Status { get; set; } = null!;

    public Claim Claim { get; set; } = null!;
}

