using FluentValidation;

namespace RaphCare.Application.Features.Insurance.Commands.UpdateInsuranceProfile;

public class UpdateInsuranceProfileValidator : AbstractValidator<UpdateInsuranceProfileCommand>
{
    public UpdateInsuranceProfileValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

