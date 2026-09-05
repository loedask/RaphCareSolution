namespace RaphCare.Client.Models.MentalHealth;

/// <summary>Maps API <c>PatientMentalHealthContentDto</c> for the MAUI hub.</summary>
public sealed class PatientMentalHealthContentViewModel
{
    public string InsightTitle { get; set; } = string.Empty;
    public string InsightBody { get; set; } = string.Empty;
    public string MedicalDisclaimer { get; set; } = string.Empty;
}

/// <summary>One mood check-in (score only; notes are not returned to the phone).</summary>
public sealed class PatientMoodCheckInViewModel
{
    public Guid Id { get; set; }
    public DateTime LoggedAt { get; set; }
    public int MoodScore { get; set; }
}
