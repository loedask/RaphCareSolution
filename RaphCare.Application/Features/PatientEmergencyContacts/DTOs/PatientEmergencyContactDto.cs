namespace RaphCare.Application.Features.PatientEmergencyContacts.DTOs;

/// <summary>ICE contact for the signed-in patient (<c>api/patient/emergency-contacts</c>).</summary>
public sealed class PatientEmergencyContactDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
