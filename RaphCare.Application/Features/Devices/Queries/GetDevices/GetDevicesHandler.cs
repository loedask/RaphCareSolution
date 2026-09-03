using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Devices.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.Devices.Queries.GetDevices;

public class GetDevicesHandler : IRequestHandler<GetDevicesQuery, PagedResult<DeviceDto>>
{
    private readonly IRepository<Device> _repository;

    public GetDevicesHandler(IRepository<Device> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<DeviceDto>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);
        var serialFilter = string.IsNullOrWhiteSpace(request.SerialContains)
            ? null
            : request.SerialContains.Trim();

        var page = await _repository.SearchAsync(
            q =>
            {
                if (request.ClinicId is Guid clinicId)
                    q = q.Where(d => d.ClinicId == clinicId);
                if (request.UnassignedOnly == true)
                    q = q.Where(d => !d.IsAssigned);
                if (serialFilter is not null)
                    q = q.Where(d => d.SerialNumber.Contains(serialFilter));
                return q.OrderByDescending(d => d.CreatedAt);
            },
            pageNumber,
            pageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return new PagedResult<DeviceDto>
        {
            Items = page.Items.Select(d => new DeviceDto
            {
                Id = d.Id,
                ClinicId = d.ClinicId,
                SerialNumber = d.SerialNumber,
                Model = d.Model,
                IsActive = d.IsActive,
                IsAssigned = d.IsAssigned,
                Status = d.Status
            }).ToList(),
            TotalCount = page.TotalCount,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        };
    }
}
