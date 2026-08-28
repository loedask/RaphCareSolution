using FluentValidation;

namespace RaphCare.Application.Features.PatientAccount.Commands.ChangeMyEmailPassword;

public sealed class ChangeMyEmailPasswordValidator : AbstractValidator<ChangeMyEmailPasswordCommand>
{
    public ChangeMyEmailPasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
