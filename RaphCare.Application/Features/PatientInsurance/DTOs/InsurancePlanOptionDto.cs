namespace RaphCare.Application.Features.PatientInsurance.DTOs;

/// <summary>Minimal plan row for patient plan selection.</summary>
public class InsurancePlanOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
