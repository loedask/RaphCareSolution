namespace RaphCare.Infrastructure.Configuration;

/// <summary>Host configuration for patient mental health hub copy (API <c>PatientMentalHealth:Content</c>).</summary>
public sealed class PatientMentalHealthContentOptions
{
    public const string SectionName = "PatientMentalHealth:Content";

    public string InsightTitle { get; set; } = string.Empty;
    public string InsightBody { get; set; } = string.Empty;
    public string MedicalDisclaimer { get; set; } = string.Empty;
}
