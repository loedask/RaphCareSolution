using MediatR;

namespace RaphCare.Application.Features.Patients.Commands.CreatePatient;

/// <summary>Use case: register a new patient at a clinic.</summary>
public class CreatePatientCommand : IRequest<Guid>
{
    public Guid ClinicId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    /// <summary>National or government-issued health identifier (used for MPI matching).</summary>
    public string? NationalHealthId { get; set; }

    /// <summary>External source system (e.g. clinic id as string) for MPI matching and linking.</summary>
    public string? SourceSystem { get; set; }

    /// <summary>External system's patient id (e.g. clinic patient id). When set with ClinicId, a PatientExternalId is created.</summary>
    public string? ExternalId { get; set; }

    /// <summary>Phone number (used for MPI matching and stored on patient when creating).</summary>
    public string? PhoneNumber { get; set; }
}

