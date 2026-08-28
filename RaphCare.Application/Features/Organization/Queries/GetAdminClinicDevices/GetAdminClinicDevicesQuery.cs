using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicDevices;

public sealed class GetAdminClinicDevicesQuery : IRequest<IReadOnlyList<AdminClinicDeviceListItemDto>?>
{
    public Guid ClinicId { get; set; }
}
