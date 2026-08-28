using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientEmergencyContacts.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientEmergencyContacts.Queries.GetMyPatientEmergencyContacts;

public sealed class GetMyPatientEmergencyContactsHandler(
    IRepository<EmergencyContact> contacts,
    ICurrentUserService currentUser) : IRequestHandler<GetMyPatientEmergencyContactsQuery, IReadOnlyList<PatientEmergencyContactDto>>
{
    private readonly IRepository<EmergencyContact> _contacts = contacts;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<IReadOnlyList<PatientEmergencyContactDto>> Handle(
        GetMyPatientEmergencyContactsQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var page = await _contacts.SearchAsync(
            q => q.Where(c => c.PatientId == patientId).OrderBy(c => c.Name),
            1,
            200,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return page.Items.Select(PatientEmergencyContactMappings.ToDto).ToList();
    }
}
