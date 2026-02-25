using MediatR;

namespace RaphCare.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommand : IRequest
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

