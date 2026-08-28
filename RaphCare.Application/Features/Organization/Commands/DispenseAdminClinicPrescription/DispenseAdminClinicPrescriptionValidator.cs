using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.DispenseAdminClinicPrescription;

public sealed class DispenseAdminClinicPrescriptionValidator : AbstractValidator<DispenseAdminClinicPrescriptionCommand>
{
    public DispenseAdminClinicPrescriptionValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PrescriptionId).NotEmpty();
    }
}
