using FluentValidation;

namespace RaphCare.Application.Features.Organization.Queries.GetCollectionDisplay;

public sealed class GetCollectionDisplayValidator : AbstractValidator<GetCollectionDisplayQuery>
{
    public GetCollectionDisplayValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(12);
    }
}
