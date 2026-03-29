using FluentValidation;

namespace RaphCare.Application.Features.PatientInsurance.Commands.CreatePatientInsuranceProfile;

public class CreatePatientInsuranceProfileValidator : AbstractValidator<CreatePatientInsuranceProfileCommand>
{
    public CreatePatientInsuranceProfileValidator()
    {
        RuleFor(x => x.InsurancePlanId).NotEmpty();
        RuleFor(x => x.MembershipNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartDate).LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
    }
}
