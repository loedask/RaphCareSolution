using FluentValidation;

namespace RaphCare.Application.Features.Devices.Commands.CreateDevice;

public class CreateDeviceValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DeviceTypeId).NotEmpty();
        RuleFor(x => x.DeviceManufacturerId).NotEmpty();
    }
}

