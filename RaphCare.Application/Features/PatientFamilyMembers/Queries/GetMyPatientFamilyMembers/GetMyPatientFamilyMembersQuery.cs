using MediatR;
using RaphCare.Application.Features.PatientFamilyMembers.DTOs;

namespace RaphCare.Application.Features.PatientFamilyMembers.Queries.GetMyPatientFamilyMembers;

public sealed class GetMyPatientFamilyMembersQuery : IRequest<IReadOnlyList<PatientFamilyMemberDto>>
{
}
