namespace RaphCare.Mobile.Core.Shared.Configuration;

/// <summary>Voice onboarding and related mobile-only configuration (not Entra).</summary>
public sealed class OnboardingOptions
{
    public const string SectionName = "Onboarding";

    /// <summary>Clinic id for POST <c>api/onboarding/voice</c>. Set to a real clinic Guid from your environment (e.g. after ClinicalSeeder).</summary>
    public Guid? VoiceRegistrationClinicId { get; set; }

    /// <summary>BCP-47 / locale tag sent with voice upload (e.g. en-ZA).</summary>
    public string DefaultVoiceLanguage { get; set; } = "en-ZA";
}
