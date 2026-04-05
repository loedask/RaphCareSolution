using FluentValidation;

namespace RaphCare.Application.Features.PatientBilling.Commands.UpgradeMyBillingPlan;

public sealed class UpgradeMyBillingPlanValidator : AbstractValidator<UpgradeMyBillingPlanCommand>
{
    public UpgradeMyBillingPlanValidator()
    {
        RuleFor(x => x.PlanCode).NotEmpty().MaximumLength(64);
    }
}
