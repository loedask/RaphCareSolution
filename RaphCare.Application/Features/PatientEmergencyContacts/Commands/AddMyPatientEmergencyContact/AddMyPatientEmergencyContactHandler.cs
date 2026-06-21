using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientEmergencyContacts.Commands.AddMyPatientEmergencyContact;

public sealed class AddMyPatientEmergencyContactHandler(
    IRepository<EmergencyContact> contacts,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<AddMyPatientEmergencyContactCommand, Guid>
{
    private readonly IRepository<EmergencyContact> _contacts = contacts;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Guid> Handle(AddMyPatientEmergencyContactCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var entity = new EmergencyContact
        {
            PatientId = patientId,
            Name = request.Name.Trim(),
            Relationship = string.IsNullOrWhiteSpace(request.Relationship) ? null : request.Relationship.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
        };

        await _contacts.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}
