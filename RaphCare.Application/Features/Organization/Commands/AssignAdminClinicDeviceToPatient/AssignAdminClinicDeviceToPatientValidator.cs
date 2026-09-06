using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.AssignAdminClinicDeviceToPatient;

public sealed class AssignAdminClinicDeviceToPatientValidator : AbstractValidator<AssignAdminClinicDeviceToPatientCommand>
{
    public AssignAdminClinicDeviceToPatientValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
    }
}
