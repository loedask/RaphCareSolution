using MediatR;
using RaphCare.Application.Features.Patients.DTOs;

namespace RaphCare.Application.Features.Patients.Queries.GetPatientById;

public class GetPatientByIdQuery : IRequest<PatientDto>
{
    public Guid Id { get; set; }
}

