using FluentValidation;

namespace RaphCare.Application.Features.PatientDevices.Commands.BindMyDeviceBluetoothMac;

public sealed class BindMyDeviceBluetoothMacValidator : AbstractValidator<BindMyDeviceBluetoothMacCommand>
{
    public BindMyDeviceBluetoothMacValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.BluetoothMacAddress).NotEmpty().MaximumLength(Common.Devices.BluetoothMacAddress.MaxLength);
    }
}
