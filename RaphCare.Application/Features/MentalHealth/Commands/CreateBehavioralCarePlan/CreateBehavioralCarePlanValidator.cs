using FluentValidation;

namespace RaphCare.Application.Features.MentalHealth.Commands.CreateBehavioralCarePlan;

public sealed class CreateBehavioralCarePlanValidator : AbstractValidator<CreateBehavioralCarePlanCommand>
{
    public CreateBehavioralCarePlanValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleForEach(x => x.Goals).ChildRules(goal =>
        {
            goal.RuleFor(g => g.GoalDescription).NotEmpty().MaximumLength(1000);
            goal.RuleFor(g => g.TargetDate).NotEmpty();
        });
    }
}
