namespace RaphCare.Application.Features.PatientBilling.DTOs;

public sealed class PatientBillingPlanOptionDto
{
    public string PlanCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Tier { get; set; }
    public decimal MonthlyPrice { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string Description { get; set; } = string.Empty;
}

public sealed class PatientCarePlanDto
{
    public string PlanCode { get; set; } = string.Empty;
    public string PlanDisplayName { get; set; } = string.Empty;
    public int Tier { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? RenewsOn { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class PatientPaymentMethodDto
{
    public Guid Id { get; set; }
    public string MethodType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string MaskedDetails { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public sealed class PatientInvoiceHistoryItemDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid? VisitId { get; set; }
}
