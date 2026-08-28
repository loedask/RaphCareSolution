using MediatR;

namespace RaphCare.Application.Features.PatientEmergencyContacts.Commands.RemoveMyPatientEmergencyContact;

public sealed class RemoveMyPatientEmergencyContactCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}
