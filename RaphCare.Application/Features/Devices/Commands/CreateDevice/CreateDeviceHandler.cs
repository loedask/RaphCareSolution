using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Devices;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.Devices.Commands.CreateDevice;

public class CreateDeviceHandler : IRequestHandler<CreateDeviceCommand, Guid>
{
    private readonly IRepository<Device> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDeviceHandler(
        IRepository<Device> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var serial = request.SerialNumber.Trim();
        var existing = await _repository.SearchAsync(
            q => q.Where(d => d.SerialNumber == serial),
            1,
            1,
            false,
            cancellationToken).ConfigureAwait(false);

        if (existing.Items.Count > 0)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(CreateDeviceCommand.SerialNumber), "A device with this serial number is already in the fleet.")
            ]);
        }

        var typeId = request.DeviceTypeId == Guid.Empty
            ? KnownDeviceCatalogIds.WearableBleDeviceTypeId
            : request.DeviceTypeId;
        var manufacturerId = request.DeviceManufacturerId == Guid.Empty
            ? KnownDeviceCatalogIds.GenericOemManufacturerId
            : request.DeviceManufacturerId;

        var device = new Device
        {
            ClinicId = request.ClinicId,
            SerialNumber = serial,
            Model = request.Model.Trim(),
            DeviceTypeId = typeId,
            DeviceManufacturerId = manufacturerId,
            IsActive = true,
            IsAssigned = false,
            Status = "InStock"
        };

        await _repository.AddAsync(device, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return device.Id;
    }
}
