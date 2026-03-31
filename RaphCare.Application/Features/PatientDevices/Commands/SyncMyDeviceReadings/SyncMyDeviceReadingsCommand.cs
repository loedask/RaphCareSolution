using MediatR;
using RaphCare.Application.Features.PatientDevices.DTOs;

namespace RaphCare.Application.Features.PatientDevices.Commands.SyncMyDeviceReadings;

/// <summary>Batch upload of BLE-derived vitals for a device assigned to the current patient.</summary>
public sealed class SyncMyDeviceReadingsCommand : IRequest<SyncMyDeviceReadingsResponseDto>
{
    public Guid DeviceId { get; set; }
    public List<HeartRatePointDto> HeartRates { get; set; } = new();
    public List<Spo2PointDto> Spo2 { get; set; } = new();
}
