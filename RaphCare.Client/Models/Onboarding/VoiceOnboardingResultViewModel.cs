namespace RaphCare.Client.Models.Onboarding;

public sealed class VoiceOnboardingResultViewModel
{
    public Guid PatientId { get; init; }
    public string Transcription { get; init; } = string.Empty;
}
