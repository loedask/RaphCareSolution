using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientFamilyMembers.Commands.AddMyPatientFamilyMember;

public sealed class AddMyPatientFamilyMemberHandler(
    IRepository<PatientFamilyMember> members,
    IRepository<Patient> patients,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<AddMyPatientFamilyMemberCommand, Guid>
{
    private readonly IRepository<PatientFamilyMember> _members = members;
    private readonly IRepository<Patient> _patients = patients;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Guid> Handle(AddMyPatientFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        if (request.LinkedPatientId is { } linkedId)
        {
            var linked = await _patients.GetByIdAsync(linkedId, cancellationToken).ConfigureAwait(false);
            if (linked is null)
                throw new NotFoundException(nameof(Patient), linkedId);
        }

        var entity = new PatientFamilyMember
        {
            OwnerPatientId = patientId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Relationship = request.Relationship.Trim(),
            DateOfBirth = request.DateOfBirth,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            LinkedPatientId = request.LinkedPatientId,
            IsActive = true
        };

        await _members.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}
