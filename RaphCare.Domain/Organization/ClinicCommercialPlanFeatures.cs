namespace RaphCare.Domain.Organization;

/// <summary>
/// Feature flags for a clinic's commercial site plan.
/// Practice and Clinic are outpatient; Hospital and Network unlock stay and front-of-house boards.
/// </summary>
public static class ClinicCommercialPlanFeatures
{
    public static bool HasInpatient(string? commercialPlan) => IsHospitalTier(commercialPlan);

    public static bool HasCollection(string? commercialPlan) => IsHospitalTier(commercialPlan);

    public static bool HasCasualty(string? commercialPlan) => IsHospitalTier(commercialPlan);

    public static bool HasTheatre(string? commercialPlan) => IsHospitalTier(commercialPlan);

    /// <summary>Consult waiting display for Practice, Clinic, Hospital, and Network.</summary>
    public static bool HasConsultWaiting(string? commercialPlan)
    {
        var plan = ClinicCommercialPlan.Normalize(commercialPlan);
        return plan is ClinicCommercialPlan.Practice
            or ClinicCommercialPlan.Clinic
            or ClinicCommercialPlan.Hospital
            or ClinicCommercialPlan.Network;
    }

    private static bool IsHospitalTier(string? commercialPlan)
    {
        var plan = ClinicCommercialPlan.Normalize(commercialPlan);
        return plan is ClinicCommercialPlan.Hospital or ClinicCommercialPlan.Network;
    }
}
