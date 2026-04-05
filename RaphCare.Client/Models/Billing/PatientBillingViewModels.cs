namespace RaphCare.Client.Models.Billing;

public sealed class PatientBillingPlanOptionViewModel
{
    public string PlanCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Tier { get; set; }
    public decimal MonthlyPrice { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string Description { get; set; } = string.Empty;

    public string PriceLine => MonthlyPrice <= 0 ? "Free" : $"{Currency} {MonthlyPrice:0}/mo";
}

public sealed class PatientCarePlanViewModel
{
    public string PlanCode { get; set; } = string.Empty;
    public string PlanDisplayName { get; set; } = string.Empty;
    public int Tier { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? RenewsOn { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class PatientPaymentMethodViewModel
{
    public Guid Id { get; set; }
    public string MethodType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string MaskedDetails { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public sealed class PatientInvoiceHistoryItemViewModel
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
}

public sealed class PagedPatientInvoicesViewModel
{
    public IReadOnlyList<PatientInvoiceHistoryItemViewModel> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
