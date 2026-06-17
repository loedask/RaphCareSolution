using FluentValidation;

namespace RaphCare.Application.Features.Clinical.Commands.UpdateVisit;

public class UpdateVisitValidator : AbstractValidator<UpdateVisitCommand>
{
    public UpdateVisitValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

