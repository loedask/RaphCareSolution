using MediatR;
using RaphCare.Application.Features.PatientEmergencyContacts.DTOs;

namespace RaphCare.Application.Features.PatientEmergencyContacts.Queries.GetMyPatientEmergencyContacts;

public sealed class GetMyPatientEmergencyContactsQuery : IRequest<IReadOnlyList<PatientEmergencyContactDto>>;
