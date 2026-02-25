using FluentValidation;

namespace RaphCare.Application.Features.Insurance.Commands.CreateInsuranceProfile;

public class CreateInsuranceProfileValidator : AbstractValidator<CreateInsuranceProfileCommand>
{
    public CreateInsuranceProfileValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.InsurancePlanId).NotEmpty();
        RuleFor(x => x.MembershipNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartDate).LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
    }
}

