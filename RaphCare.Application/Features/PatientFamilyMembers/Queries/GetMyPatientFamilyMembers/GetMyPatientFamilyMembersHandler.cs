using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientFamilyMembers.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientFamilyMembers.Queries.GetMyPatientFamilyMembers;

public sealed class GetMyPatientFamilyMembersHandler(
    IRepository<PatientFamilyMember> members,
    ICurrentUserService currentUser) : IRequestHandler<GetMyPatientFamilyMembersQuery, IReadOnlyList<PatientFamilyMemberDto>>
{
    private readonly IRepository<PatientFamilyMember> _members = members;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<IReadOnlyList<PatientFamilyMemberDto>> Handle(GetMyPatientFamilyMembersQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var page = await _members.SearchAsync(
            q => q.Where(m => m.OwnerPatientId == patientId && m.IsActive)
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName),
            1,
            200,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return page.Items.Select(PatientFamilyMemberMappings.ToDto).ToList();
    }
}
