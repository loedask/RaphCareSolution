using FluentValidation;

namespace RaphCare.Application.Features.AI.Commands.GenerateSummary;

public class GenerateSummaryValidator : AbstractValidator<GenerateSummaryCommand>
{
    public GenerateSummaryValidator()
    {
        RuleFor(x => x.SourceType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SourceId).NotEmpty();
    }
}

