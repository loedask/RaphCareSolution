using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Maps domain appointment entities to a minimal FHIR-shaped DTO for export.
/// </summary>
public interface IAppointmentFhirMapper
{
    Task<FhirAppointmentDto> MapToDtoAsync(Appointment appointment, CancellationToken ct);
}

