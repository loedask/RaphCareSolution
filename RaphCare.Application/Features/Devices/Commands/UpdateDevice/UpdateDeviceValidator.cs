using FluentValidation;

namespace RaphCare.Application.Features.Devices.Commands.UpdateDevice;

public class UpdateDeviceValidator : AbstractValidator<UpdateDeviceCommand>
{
    public UpdateDeviceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

