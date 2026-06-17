using MediatR;

namespace RaphCare.Application.Features.PatientProfile.Commands.UpdateMyPatientProfile;

/// <summary>Self-service update of demographics for the JWT <c>patientId</c> claim.</summary>
public sealed class UpdateMyPatientProfileCommand : IRequest<Unit>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
}
