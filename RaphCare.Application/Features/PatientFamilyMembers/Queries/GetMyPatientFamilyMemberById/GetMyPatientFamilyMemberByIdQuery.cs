using MediatR;
using RaphCare.Application.Features.PatientFamilyMembers.DTOs;

namespace RaphCare.Application.Features.PatientFamilyMembers.Queries.GetMyPatientFamilyMemberById;

public sealed class GetMyPatientFamilyMemberByIdQuery : IRequest<PatientFamilyMemberDto>
{
    public Guid Id { get; set; }
}
