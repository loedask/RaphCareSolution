using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicPrescription;

public sealed class CallAdminClinicPrescriptionValidator : AbstractValidator<CallAdminClinicPrescriptionCommand>
{
    public CallAdminClinicPrescriptionValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PrescriptionId).NotEmpty();
    }
}
