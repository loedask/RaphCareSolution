using MediatR;
using RaphCare.Application.Features.Interoperability.DTOs;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirPatientById;

/// <summary>
/// Phase 1: exports a single patient as a minimal FHIR (Fast Healthcare Interoperability Resources)-shaped DTO.
/// </summary>
public class GetFhirPatientByIdQuery : IRequest<FhirPatientDto>
{
    public Guid Id { get; set; }
}

