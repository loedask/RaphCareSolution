using MediatR;

namespace RaphCare.Application.Features.PatientEmergencyContacts.Commands.UpdateMyPatientEmergencyContact;

public sealed class UpdateMyPatientEmergencyContactCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
