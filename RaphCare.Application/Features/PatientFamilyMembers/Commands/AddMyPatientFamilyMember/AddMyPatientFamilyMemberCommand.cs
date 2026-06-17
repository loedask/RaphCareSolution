using MediatR;

namespace RaphCare.Application.Features.PatientFamilyMembers.Commands.AddMyPatientFamilyMember;

public sealed class AddMyPatientFamilyMemberCommand : IRequest<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid? LinkedPatientId { get; set; }
}
