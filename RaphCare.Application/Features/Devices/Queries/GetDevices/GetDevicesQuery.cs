using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Devices.DTOs;

namespace RaphCare.Application.Features.Devices.Queries.GetDevices;

public class GetDevicesQuery : IRequest<PagedResult<DeviceDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

