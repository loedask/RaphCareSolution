using MediatR;
using RaphCare.Application.Features.Interoperability.DTOs;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirEncounterById;

/// <summary>
/// Phase 1: exports a Visit as a minimal FHIR-shaped Encounter DTO.
/// </summary>
public class GetFhirEncounterByIdQuery : IRequest<FhirEncounterDto>
{
    public Guid Id { get; set; }
}

