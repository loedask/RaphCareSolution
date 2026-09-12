using FluentValidation;

namespace RaphCare.Application.Features.Billing.Commands.UpdatePriceCatalogItem;

public sealed class UpdatePriceCatalogItemValidator : AbstractValidator<UpdatePriceCatalogItemCommand>
{
    public UpdatePriceCatalogItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AmountZar).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AmountUsd).GreaterThanOrEqualTo(0);
    }
}
