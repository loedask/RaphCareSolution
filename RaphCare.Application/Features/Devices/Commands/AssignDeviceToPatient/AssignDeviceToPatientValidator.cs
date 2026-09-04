using FluentValidation;

namespace RaphCare.Application.Features.Devices.Commands.AssignDeviceToPatient;

public sealed class AssignDeviceToPatientValidator : AbstractValidator<AssignDeviceToPatientCommand>
{
    public AssignDeviceToPatientValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
    }
}
