using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Maps domain patient entities to a minimal FHIR (Fast Healthcare Interoperability Resources)-shaped DTO for export.
/// </summary>
public interface IPatientFhirMapper
{
    Task<FhirPatientDto> MapToDtoAsync(Patient patient, CancellationToken ct);
}

