using MediatR;
using RaphCare.Application.Features.PatientEmergencyContacts.DTOs;

namespace RaphCare.Application.Features.PatientEmergencyContacts.Queries.GetMyPatientEmergencyContactById;

public sealed class GetMyPatientEmergencyContactByIdQuery : IRequest<PatientEmergencyContactDto>
{
    public Guid Id { get; init; }
}
