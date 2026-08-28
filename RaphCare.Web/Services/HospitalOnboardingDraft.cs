namespace RaphCare.Web.Services;

/// <summary>Persisted hospital onboarding wizard state (browser localStorage).</summary>
public sealed class HospitalOnboardingDraft
{
    public int Step { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Country { get; set; } = "South Africa";
    public string TimeZone { get; set; } = "South Africa Standard Time";
    public string FacilityName { get; set; } = string.Empty;
    public string FacilityAddress { get; set; } = string.Empty;
    public string FacilityCity { get; set; } = string.Empty;
    public bool IsVirtualFacility { get; set; }
    public string AdminFullName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public DateTimeOffset SavedAt { get; set; }

    public bool HasHospitalDetails =>
        !string.IsNullOrWhiteSpace(Name)
        || !string.IsNullOrWhiteSpace(RegistrationNumber)
        || !string.IsNullOrWhiteSpace(FacilityName);
}
