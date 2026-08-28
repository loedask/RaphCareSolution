using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientEmergencyContacts.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientEmergencyContacts.Queries.GetMyPatientEmergencyContactById;

public sealed class GetMyPatientEmergencyContactByIdHandler(
    IRepository<EmergencyContact> contacts,
    ICurrentUserService currentUser) : IRequestHandler<GetMyPatientEmergencyContactByIdQuery, PatientEmergencyContactDto>
{
    private readonly IRepository<EmergencyContact> _contacts = contacts;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<PatientEmergencyContactDto> Handle(
        GetMyPatientEmergencyContactByIdQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var entity = await _contacts.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null || entity.PatientId != patientId)
            throw new NotFoundException(nameof(EmergencyContact), request.Id);

        return PatientEmergencyContactMappings.ToDto(entity);
    }
}
