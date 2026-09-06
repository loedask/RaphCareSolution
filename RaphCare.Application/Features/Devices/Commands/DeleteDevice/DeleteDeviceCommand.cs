using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Devices.Commands.DeleteDevice;

/// <summary>Removes or retires a fleet device (platform ops only). Assigned devices must be revoked first.</summary>
public sealed class DeleteDeviceCommand : IRequest, IPlatformAdminRequest
{
    public Guid DeviceId { get; set; }
}
