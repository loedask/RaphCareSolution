namespace RaphCare.Client.Models.MentalHealth;

/// <summary>Maps API <c>PatientMentalHealthContentDto</c> for the MAUI hub.</summary>
public sealed class PatientMentalHealthContentViewModel
{
    public string InsightTitle { get; set; } = string.Empty;
    public string InsightBody { get; set; } = string.Empty;
    public string MedicalDisclaimer { get; set; } = string.Empty;
}
