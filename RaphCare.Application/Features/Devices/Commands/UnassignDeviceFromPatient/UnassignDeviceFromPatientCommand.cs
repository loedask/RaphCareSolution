using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Devices.Commands.UnassignDeviceFromPatient;

/// <summary>Ends the active patient assignment and returns the device to in-stock (platform ops).</summary>
public sealed class UnassignDeviceFromPatientCommand : IRequest<Guid>, IPlatformAdminRequest
{
    public Guid DeviceId { get; set; }
}
