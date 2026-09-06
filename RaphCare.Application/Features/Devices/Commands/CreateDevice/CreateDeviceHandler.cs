using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Devices;
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

        string? mac;
        try
        {
            mac = BluetoothMacAddress.NormalizeOrNull(request.BluetoothMacAddress);
        }
        catch (FormatException)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(CreateDeviceCommand.BluetoothMacAddress),
                    "Enter a valid Bluetooth MAC (for example AA:BB:CC:DD:EE:FF).")
            ]);
        }

        if (mac is not null)
        {
            var macConflict = await _repository.SearchAsync(
                q => q.Where(d => d.BluetoothMacAddress == mac),
                1,
                1,
                false,
                cancellationToken).ConfigureAwait(false);

            if (macConflict.Items.Count > 0)
            {
                throw new ValidationException(
                [
                    new ValidationFailure(
                        nameof(CreateDeviceCommand.BluetoothMacAddress),
                        "A device with this Bluetooth MAC is already in the fleet.")
                ]);
            }
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
            BluetoothMacAddress = mac,
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
