using MediatR;

namespace RaphCare.Application.Features.Patients.Commands.CreatePatient;

/// <summary>Use case: register a new patient at a clinic.</summary>
public class CreatePatientCommand : IRequest<Guid>
{
    public Guid ClinicId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

