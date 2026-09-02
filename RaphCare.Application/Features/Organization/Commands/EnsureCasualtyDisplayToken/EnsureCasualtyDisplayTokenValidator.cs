using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.EnsureCasualtyDisplayToken;

public sealed class EnsureCasualtyDisplayTokenValidator : AbstractValidator<EnsureCasualtyDisplayTokenCommand>
{
    public EnsureCasualtyDisplayTokenValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
    }
}
