using MediatR;

namespace RaphCare.Application.Features.PatientFamilyMembers.Commands.RemoveMyPatientFamilyMember;

public sealed class RemoveMyPatientFamilyMemberCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
