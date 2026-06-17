using MediatR;
using RaphCare.Application.Features.Devices.DTOs;

namespace RaphCare.Application.Features.Devices.Queries.GetDeviceById;

public class GetDeviceByIdQuery : IRequest<DeviceDto>
{
    public Guid Id { get; set; }
}

