using FluentValidation;

namespace RaphCare.Application.Features.Devices.Commands.SetDeviceBluetoothMac;

public sealed class SetDeviceBluetoothMacValidator : AbstractValidator<SetDeviceBluetoothMacCommand>
{
    public SetDeviceBluetoothMacValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.BluetoothMacAddress).NotEmpty().MaximumLength(Common.Devices.BluetoothMacAddress.MaxLength);
    }
}
