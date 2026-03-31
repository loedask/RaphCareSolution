using FluentValidation;

namespace RaphCare.Application.Features.PatientDevices.Commands.RegisterMyDevice;

public sealed class RegisterMyDeviceValidator : AbstractValidator<RegisterMyDeviceCommand>
{
    public RegisterMyDeviceValidator()
    {
        RuleFor(x => x.SerialNumber)
            .NotEmpty()
            .MaximumLength(100)
            .Must(s => !string.IsNullOrWhiteSpace(s.Trim()));
        RuleFor(x => x.ModelSku).NotEmpty().MaximumLength(100);
    }
}
