using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.EnsureCollectionDisplayToken;

public sealed class EnsureCollectionDisplayTokenValidator : AbstractValidator<EnsureCollectionDisplayTokenCommand>
{
    public EnsureCollectionDisplayTokenValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
    }
}
