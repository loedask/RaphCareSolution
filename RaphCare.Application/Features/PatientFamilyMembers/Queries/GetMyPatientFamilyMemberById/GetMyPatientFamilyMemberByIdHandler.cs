using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientFamilyMembers.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientFamilyMembers.Queries.GetMyPatientFamilyMemberById;

public sealed class GetMyPatientFamilyMemberByIdHandler(
    IRepository<PatientFamilyMember> members,
    ICurrentUserService currentUser) : IRequestHandler<GetMyPatientFamilyMemberByIdQuery, PatientFamilyMemberDto>
{
    private readonly IRepository<PatientFamilyMember> _members = members;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<PatientFamilyMemberDto> Handle(GetMyPatientFamilyMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var entity = await _members.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null || entity.OwnerPatientId != patientId || !entity.IsActive)
            throw new NotFoundException(nameof(PatientFamilyMember), request.Id);

        return PatientFamilyMemberMappings.ToDto(entity);
    }
}
