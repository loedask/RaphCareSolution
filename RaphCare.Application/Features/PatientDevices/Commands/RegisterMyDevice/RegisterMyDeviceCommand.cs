using MediatR;
using RaphCare.Application.Features.PatientDevices.DTOs;

namespace RaphCare.Application.Features.PatientDevices.Commands.RegisterMyDevice;

/// <summary>Claims a fleet wearable already assigned to the current patient (serial must exist in inventory).</summary>
public sealed class RegisterMyDeviceCommand : IRequest<RegisterMyDeviceResponseDto>
{
    public string SerialNumber { get; set; } = string.Empty;
    public string ModelSku { get; set; } = string.Empty;
}
