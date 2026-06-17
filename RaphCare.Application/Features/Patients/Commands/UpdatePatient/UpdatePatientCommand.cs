using MediatR;

namespace RaphCare.Application.Features.Patients.Commands.UpdatePatient;

/// <summary>
/// Use case: update basic patient profile fields (e.g. names).
/// </summary>
public class UpdatePatientCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

