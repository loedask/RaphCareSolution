using FluentValidation;

namespace RaphCare.Application.Features.PatientInsurance.Commands.UpdateMyInsuranceProfile;

public class UpdateMyInsuranceProfileValidator : AbstractValidator<UpdateMyInsuranceProfileCommand>
{
    public UpdateMyInsuranceProfileValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
