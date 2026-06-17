using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientFamilyMembers.Commands.UpdateMyPatientFamilyMember;

public sealed class UpdateMyPatientFamilyMemberHandler(
    IRepository<PatientFamilyMember> members,
    IRepository<Patient> patients,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<UpdateMyPatientFamilyMemberCommand, Unit>
{
    private readonly IRepository<PatientFamilyMember> _members = members;
    private readonly IRepository<Patient> _patients = patients;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Unit> Handle(UpdateMyPatientFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var entity = await _members.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null || entity.OwnerPatientId != patientId || !entity.IsActive)
            throw new NotFoundException(nameof(PatientFamilyMember), request.Id);

        if (request.LinkedPatientId is { } linkedId)
        {
            var linked = await _patients.GetByIdAsync(linkedId, cancellationToken).ConfigureAwait(false);
            if (linked is null)
                throw new NotFoundException(nameof(Patient), linkedId);
        }

        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Relationship = request.Relationship.Trim();
        entity.DateOfBirth = request.DateOfBirth;
        entity.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        entity.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        entity.LinkedPatientId = request.LinkedPatientId;

        await _members.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
