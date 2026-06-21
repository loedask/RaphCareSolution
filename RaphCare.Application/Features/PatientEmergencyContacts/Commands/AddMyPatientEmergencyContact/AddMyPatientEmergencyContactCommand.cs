using MediatR;

namespace RaphCare.Application.Features.PatientEmergencyContacts.Commands.AddMyPatientEmergencyContact;

public sealed class AddMyPatientEmergencyContactCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
