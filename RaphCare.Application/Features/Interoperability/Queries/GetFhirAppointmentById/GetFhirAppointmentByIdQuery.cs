using MediatR;
using RaphCare.Application.Features.Interoperability.DTOs;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirAppointmentById;

/// <summary>
/// Phase 1: exports a single Appointment as a minimal FHIR-shaped DTO.
/// </summary>
public class GetFhirAppointmentByIdQuery : IRequest<FhirAppointmentDto>
{
    public Guid Id { get; set; }
}

