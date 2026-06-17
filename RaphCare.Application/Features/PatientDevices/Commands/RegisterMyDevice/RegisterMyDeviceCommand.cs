using MediatR;
using RaphCare.Application.Features.PatientDevices.DTOs;

namespace RaphCare.Application.Features.PatientDevices.Commands.RegisterMyDevice;

/// <summary>Registers or links a patient-provided wearable (e.g. E580/E585) by serial number for the current patient.</summary>
public sealed class RegisterMyDeviceCommand : IRequest<RegisterMyDeviceResponseDto>
{
    public string SerialNumber { get; set; } = string.Empty;
    public string ModelSku { get; set; } = string.Empty;
}
