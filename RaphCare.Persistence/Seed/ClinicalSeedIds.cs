namespace RaphCare.Persistence.Seed;

/// <summary>Stable identifiers for local demo data (mobile <c>Appointments</c> defaults, telehealth E2E).</summary>
public static class ClinicalSeedIds
{
    public static readonly Guid DemoClinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    public static readonly Guid DemoProviderId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    public static readonly Guid DemoProviderApplicationUserId = Guid.Parse("11111111-1111-1111-1111-111111111103");
    public static readonly Guid DemoPatientId = Guid.Parse("11111111-1111-1111-1111-111111111104");
    public static readonly Guid DemoMentalHealthAssessmentId = Guid.Parse("11111111-1111-1111-1111-111111111105");
}
