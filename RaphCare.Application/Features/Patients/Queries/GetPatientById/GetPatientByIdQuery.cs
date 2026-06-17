using MediatR;
using RaphCare.Application.Features.Patients.DTOs;

namespace RaphCare.Application.Features.Patients.Queries.GetPatientById;

/// <summary>Use case: retrieve a single patient by id for display or edit.</summary>
public class GetPatientByIdQuery : IRequest<PatientDto>
{
    public Guid Id { get; set; }
}

