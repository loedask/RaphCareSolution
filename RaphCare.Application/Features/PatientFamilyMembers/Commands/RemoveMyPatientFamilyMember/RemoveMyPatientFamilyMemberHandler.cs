using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientFamilyMembers.Commands.RemoveMyPatientFamilyMember;

public sealed class RemoveMyPatientFamilyMemberHandler(
    IRepository<PatientFamilyMember> members,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<RemoveMyPatientFamilyMemberCommand, Unit>
{
    private readonly IRepository<PatientFamilyMember> _members = members;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Unit> Handle(RemoveMyPatientFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var entity = await _members.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null || entity.OwnerPatientId != patientId || !entity.IsActive)
            throw new NotFoundException(nameof(PatientFamilyMember), request.Id);

        entity.IsActive = false;
        await _members.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
