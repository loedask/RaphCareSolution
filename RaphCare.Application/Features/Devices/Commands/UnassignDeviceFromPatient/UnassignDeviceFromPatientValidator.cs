using FluentValidation;

namespace RaphCare.Application.Features.Devices.Commands.UnassignDeviceFromPatient;

public sealed class UnassignDeviceFromPatientValidator : AbstractValidator<UnassignDeviceFromPatientCommand>
{
    public UnassignDeviceFromPatientValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty();
    }
}
