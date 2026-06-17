using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Billing;

public class PaymentMethod : BaseEntity
{
    public Guid PatientId { get; set; }

    public string MethodType { get; set; } = null!;
    public string ProviderName { get; set; } = null!;
    public string MaskedDetails { get; set; } = null!;

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }

    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}

