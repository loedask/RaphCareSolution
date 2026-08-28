namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicPatientListItemDto
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string AccessType { get; set; } = string.Empty;
    public DateTime GrantedAt { get; set; }
}
