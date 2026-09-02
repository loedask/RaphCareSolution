using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.DischargeAdminClinicAdmission;

public sealed class DischargeAdminClinicAdmissionValidator : AbstractValidator<DischargeAdminClinicAdmissionCommand>
{
    public DischargeAdminClinicAdmissionValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.AdmissionId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.DischargeSummary).MaximumLength(4000);
        RuleFor(x => x.NightlyBedRate).GreaterThanOrEqualTo(0).When(x => x.NightlyBedRate.HasValue);
        RuleFor(x => x.ExtraAmount).GreaterThanOrEqualTo(0).When(x => x.ExtraAmount.HasValue);
        RuleFor(x => x.ExtraDescription).MaximumLength(200);
        RuleFor(x => x.Currency)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));
    }
}
