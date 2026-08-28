namespace RaphCare.Application.Features.PatientMedicalInfo.DTOs;

/// <summary>Patient-reported medical summary (<c>api/patient/medical-info</c>).</summary>
public sealed class MyPatientMedicalInfoDto
{
    public string BloodType { get; set; } = string.Empty;
    public string Allergies { get; set; } = string.Empty;
    public string ChronicConditions { get; set; } = string.Empty;
    public string Medications { get; set; } = string.Empty;
    public string PrimaryDoctor { get; set; } = string.Empty;
}
