using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Billing;

public class InvoiceLineItem : BaseEntity
{
    public Guid InvoiceId { get; set; }

    public string ServiceType { get; set; } = null!;
    public string Description { get; set; } = null!;

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public string? ReferenceId { get; set; }

    public Invoice? Invoice { get; set; }
}

