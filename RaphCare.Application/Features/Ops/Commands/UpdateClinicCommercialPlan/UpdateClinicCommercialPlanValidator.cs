using FluentValidation;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Ops.Commands.UpdateClinicCommercialPlan;

public sealed class UpdateClinicCommercialPlanValidator : AbstractValidator<UpdateClinicCommercialPlanCommand>
{
    public UpdateClinicCommercialPlanValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.CommercialPlan)
            .NotEmpty()
            .Must(ClinicCommercialPlan.IsKnown)
            .WithMessage("Commercial plan must be Practice, Clinic, Hospital, or Network.");
    }
}
