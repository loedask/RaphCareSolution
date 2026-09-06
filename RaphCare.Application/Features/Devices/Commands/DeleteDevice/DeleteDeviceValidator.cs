using FluentValidation;

namespace RaphCare.Application.Features.Devices.Commands.DeleteDevice;

public sealed class DeleteDeviceValidator : AbstractValidator<DeleteDeviceCommand>
{
    public DeleteDeviceValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty();
    }
}
