using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Billing;

public class BillingProfile : BaseEntity
{
    public Guid PatientId { get; set; }

    public string BillingAddress { get; set; } = null!;
    public string PreferredCurrency { get; set; } = null!;

    public bool EmailInvoices { get; set; }
    public bool SMSNotifications { get; set; }
}

