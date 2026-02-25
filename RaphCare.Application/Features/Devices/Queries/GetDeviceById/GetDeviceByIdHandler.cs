using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Devices.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.Devices.Queries.GetDeviceById;

public class GetDeviceByIdHandler : IRequestHandler<GetDeviceByIdQuery, DeviceDto>
{
    private readonly IRepository<Device> _repository;

    public GetDeviceByIdHandler(IRepository<Device> repository)
    {
        _repository = repository;
    }

    public async Task<DeviceDto> Handle(GetDeviceByIdQuery request, CancellationToken cancellationToken)
    {
        var device = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (device is null)
        {
            throw new NotFoundException(nameof(Device), request.Id);
        }

        return new DeviceDto
        {
            Id = device.Id,
            ClinicId = device.ClinicId,
            SerialNumber = device.SerialNumber,
            Model = device.Model,
            IsActive = device.IsActive,
            IsAssigned = device.IsAssigned,
            Status = device.Status
        };
    }
}

