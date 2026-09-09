using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Devices;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Devices;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.Devices.Commands.SetDeviceBluetoothMac;

public sealed class SetDeviceBluetoothMacHandler : IRequestHandler<SetDeviceBluetoothMacCommand>
{
    private readonly IRepository<Device> _devices;
    private readonly IUnitOfWork _unitOfWork;

    public SetDeviceBluetoothMacHandler(IRepository<Device> devices, IUnitOfWork unitOfWork)
    {
        _devices = devices;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetDeviceBluetoothMacCommand request, CancellationToken cancellationToken)
    {
        var device = await _devices.GetByIdAsync(request.DeviceId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Device), request.DeviceId);

        if (!device.IsActive)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(SetDeviceBluetoothMacCommand.DeviceId),
                    "This device is not active in the fleet.")
            ]);
        }

        string mac;
        try
        {
            mac = BluetoothMacAddress.NormalizeOrNull(request.BluetoothMacAddress)
                ?? throw new FormatException();
        }
        catch (FormatException)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(SetDeviceBluetoothMacCommand.BluetoothMacAddress),
                    "Enter a valid Bluetooth MAC (for example AA:BB:CC:DD:EE:FF).")
            ]);
        }

        var conflict = await _devices.SearchAsync(
            q => q.Where(d => d.BluetoothMacAddress == mac && d.Id != device.Id),
            1,
            1,
            false,
            cancellationToken).ConfigureAwait(false);

        if (conflict.Items.Count > 0)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(SetDeviceBluetoothMacCommand.BluetoothMacAddress),
                    "A device with this Bluetooth MAC is already in the fleet.")
            ]);
        }

        device.BluetoothMacAddress = mac;
        await _devices.UpdateAsync(device, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
