using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Common.Interfaces;

/// <summary>Maps <see cref="DeviceReading"/> rows to minimal FHIR Observation DTOs.</summary>
public interface IDeviceReadingFhirMapper
{
    Task<FhirObservationDto> MapToObservationAsync(DeviceReading reading, Device device, CancellationToken ct);
}
