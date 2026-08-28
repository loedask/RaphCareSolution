namespace RaphCare.Application.Features.Organization.DTOs;

/// <summary>Result of platform hospital onboarding registration.</summary>
public class RegisterClinicResultDto
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public Guid? PrimaryFacilityId { get; set; }
    public string? PrimaryFacilityName { get; set; }
}
