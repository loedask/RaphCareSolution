using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.EnsureConsultDisplayToken;

public sealed class EnsureConsultDisplayTokenValidator : AbstractValidator<EnsureConsultDisplayTokenCommand>
{
    public EnsureConsultDisplayTokenValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
    }
}
