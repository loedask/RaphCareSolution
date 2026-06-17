using RaphCare.Application.Features.PatientBilling.DTOs;

namespace RaphCare.Application.Features.PatientBilling;

/// <summary>Published care / app plan tiers (Vertical 9). Replace with DB-driven catalog when product defines pricing.</summary>
public static class PatientBillingCatalog
{
    public const string FreeCode = "FREE";
    public const string EssentialCode = "ESSENTIAL";
    public const string CompleteCode = "COMPLETE";

    public static IReadOnlyList<PatientBillingPlanOptionDto> All { get; } =
    [
        new()
        {
            PlanCode = FreeCode,
            DisplayName = "Free",
            Tier = 0,
            MonthlyPrice = 0,
            Currency = "ZAR",
            Description = "Core app access and health record viewing."
        },
        new()
        {
            PlanCode = EssentialCode,
            DisplayName = "Essential Care",
            Tier = 1,
            MonthlyPrice = 149,
            Currency = "ZAR",
            Description = "Priority messaging, extended device history, and care reminders."
        },
        new()
        {
            PlanCode = CompleteCode,
            DisplayName = "Complete Care",
            Tier = 2,
            MonthlyPrice = 299,
            Currency = "ZAR",
            Description = "Everything in Essential plus telehealth credits and family sharing (when available)."
        }
    ];

    public static PatientBillingPlanOptionDto? FindByCode(string? planCode)
    {
        if (string.IsNullOrWhiteSpace(planCode))
            return null;
        var c = planCode.Trim().ToUpperInvariant();
        return All.FirstOrDefault(p => string.Equals(p.PlanCode, c, StringComparison.Ordinal));
    }
}
