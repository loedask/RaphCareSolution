using MediatR;
using RaphCare.Application.Features.Interoperability.DTOs;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirOrganizationById;

/// <summary>
/// Phase 1: exports a clinic/practice as a minimal FHIR-shaped Organization DTO.
/// </summary>
public class GetFhirOrganizationByIdQuery : IRequest<FhirOrganizationDto>
{
    public Guid Id { get; set; }
}

