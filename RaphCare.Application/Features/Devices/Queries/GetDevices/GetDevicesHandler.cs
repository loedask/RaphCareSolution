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
        var devices = await _repository.ListAsync(cancellationToken);

        var totalCount = devices.Count;

        var items = devices
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DeviceDto
            {
                Id = d.Id,
                ClinicId = d.ClinicId,
                SerialNumber = d.SerialNumber,
                Model = d.Model,
                IsActive = d.IsActive,
                IsAssigned = d.IsAssigned,
                Status = d.Status
            })
            .ToList();

        return new PagedResult<DeviceDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

