using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Maps clinical organization entities (clinics/practices) to a minimal FHIR (Fast Healthcare Interoperability Resources)-shaped DTO for export.
/// </summary>
public interface IOrganizationFhirMapper
{
    Task<FhirOrganizationDto> MapToDtoAsync(Clinic organization, CancellationToken ct);
}

