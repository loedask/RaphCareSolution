namespace RaphCare.Application.Features.PatientMentalHealth.DTOs;

/// <summary>Configurable hub copy for concept <c>/mental-health</c> (AI insight card + safety disclaimer).</summary>
public sealed class PatientMentalHealthContentDto
{
    public string InsightTitle { get; set; } = string.Empty;
    public string InsightBody { get; set; } = string.Empty;
    public string MedicalDisclaimer { get; set; } = string.Empty;
}
