using MediatR;

namespace RaphCare.Application.Features.PatientMedicalInfo.Commands.UpdateMyPatientMedicalInfo;

/// <summary>Upserts patient-reported medical summary on <see cref="Domain.Patients.PatientProfile"/>.</summary>
public sealed class UpdateMyPatientMedicalInfoCommand : IRequest<Unit>
{
    public string BloodType { get; set; } = string.Empty;
    public string Allergies { get; set; } = string.Empty;
    public string ChronicConditions { get; set; } = string.Empty;
    public string Medications { get; set; } = string.Empty;
    public string PrimaryDoctor { get; set; } = string.Empty;
}
