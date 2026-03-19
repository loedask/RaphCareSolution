using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Maps clinical encounter entities to a minimal FHIR (Fast Healthcare Interoperability Resources)-shaped DTO for export.
/// </summary>
public interface IEncounterFhirMapper
{
    Task<FhirEncounterDto> MapToDtoAsync(Visit visit, CancellationToken ct);
}

