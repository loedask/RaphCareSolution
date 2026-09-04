using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Devices.DTOs;

namespace RaphCare.Application.Features.Devices.Queries.GetDevices;

public class GetDevicesQuery : IRequest<PagedResult<DeviceDto>>, IPlatformAdminRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ClinicId { get; set; }
    public bool? UnassignedOnly { get; set; }
    public string? SerialContains { get; set; }
}
