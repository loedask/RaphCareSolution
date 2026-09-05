using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Devices;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientDevices.DTOs;
using RaphCare.Domain.Devices;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.PatientDevices.Commands.BindMyDeviceBluetoothMac;

public sealed class BindMyDeviceBluetoothMacHandler : IRequestHandler<BindMyDeviceBluetoothMacCommand, BindMyDeviceBluetoothMacResponseDto>
{
    private readonly IRepository<Device> _devices;
    private readonly IRepository<DeviceAssignment> _assignments;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public BindMyDeviceBluetoothMacHandler(
        IRepository<Device> devices,
        IRepository<DeviceAssignment> assignments,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _devices = devices;
        _assignments = assignments;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<BindMyDeviceBluetoothMacResponseDto> Handle(
        BindMyDeviceBluetoothMacCommand request,
        CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

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
                    nameof(BindMyDeviceBluetoothMacCommand.BluetoothMacAddress),
                    "Enter a valid Bluetooth MAC (for example AA:BB:CC:DD:EE:FF).")
            ]);
        }

        var device = await _devices.GetByIdAsync(request.DeviceId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Device), request.DeviceId);

        var assignment = await _assignments.SearchAsync(
            q => q.Where(a => a.DeviceId == device.Id && a.PatientId == patientId && a.IsActive),
            1,
            1,
            true,
            cancellationToken).ConfigureAwait(false);

        if (assignment.Items.Count == 0)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(BindMyDeviceBluetoothMacCommand.DeviceId),
                    "Claim this watch before binding Bluetooth.")
            ]);
        }

        if (!string.IsNullOrWhiteSpace(device.BluetoothMacAddress))
        {
            if (BluetoothMacAddress.EqualsNormalized(device.BluetoothMacAddress, mac))
            {
                return new BindMyDeviceBluetoothMacResponseDto
                {
                    DeviceId = device.Id,
                    BluetoothMacAddress = device.BluetoothMacAddress!
                };
            }

            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(BindMyDeviceBluetoothMacCommand.BluetoothMacAddress),
                    "This watch is already locked to a different Bluetooth address.")
            ]);
        }

        var conflict = await _devices.SearchAsync(
            q => q.Where(d => d.Id != device.Id && d.BluetoothMacAddress == mac),
            1,
            1,
            false,
            cancellationToken).ConfigureAwait(false);

        if (conflict.Items.Count > 0)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(BindMyDeviceBluetoothMacCommand.BluetoothMacAddress),
                    "This Bluetooth address is already linked to another fleet watch.")
            ]);
        }

        device.BluetoothMacAddress = mac;
        await _devices.UpdateAsync(device, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new BindMyDeviceBluetoothMacResponseDto
        {
            DeviceId = device.Id,
            BluetoothMacAddress = mac
        };
    }
}
