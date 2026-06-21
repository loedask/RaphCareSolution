using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientEmergencyContacts.Commands.UpdateMyPatientEmergencyContact;

public sealed class UpdateMyPatientEmergencyContactHandler(
    IRepository<EmergencyContact> contacts,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<UpdateMyPatientEmergencyContactCommand, Unit>
{
    private readonly IRepository<EmergencyContact> _contacts = contacts;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Unit> Handle(UpdateMyPatientEmergencyContactCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var entity = await _contacts.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null || entity.PatientId != patientId)
            throw new NotFoundException(nameof(EmergencyContact), request.Id);

        entity.Name = request.Name.Trim();
        entity.Relationship = string.IsNullOrWhiteSpace(request.Relationship) ? null : request.Relationship.Trim();
        entity.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        entity.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        await _contacts.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
