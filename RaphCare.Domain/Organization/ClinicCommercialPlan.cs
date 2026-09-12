namespace RaphCare.Domain.Organization;

/// <summary>Commercial site plan codes stored on <see cref="Clinic.CommercialPlan"/>.</summary>
public static class ClinicCommercialPlan
{
    public const string Practice = "Practice";
    public const string Clinic = "Clinic";
    public const string Hospital = "Hospital";
    public const string Network = "Network";

    public const int MaxLength = 32;

    /// <summary>Default for existing rows and new clinics until Ops assigns another plan.</summary>
    public const string Default = Clinic;

    public static bool IsKnown(string? plan) =>
        string.Equals(Normalize(plan), Practice, StringComparison.OrdinalIgnoreCase)
        || string.Equals(Normalize(plan), Clinic, StringComparison.OrdinalIgnoreCase)
        || string.Equals(Normalize(plan), Hospital, StringComparison.OrdinalIgnoreCase)
        || string.Equals(Normalize(plan), Network, StringComparison.OrdinalIgnoreCase);

    /// <summary>Canonical casing for storage; unknown or empty becomes <see cref="Default"/>.</summary>
    public static string Normalize(string? plan)
    {
        if (string.IsNullOrWhiteSpace(plan))
            return Default;

        var trimmed = plan.Trim();
        if (string.Equals(trimmed, Practice, StringComparison.OrdinalIgnoreCase))
            return Practice;
        if (string.Equals(trimmed, Clinic, StringComparison.OrdinalIgnoreCase))
            return Clinic;
        if (string.Equals(trimmed, Hospital, StringComparison.OrdinalIgnoreCase))
            return Hospital;
        if (string.Equals(trimmed, Network, StringComparison.OrdinalIgnoreCase))
            return Network;
        return Default;
    }
}
