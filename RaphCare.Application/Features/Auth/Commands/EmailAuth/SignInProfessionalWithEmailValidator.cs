using FluentValidation;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class SignInProfessionalWithEmailValidator : AbstractValidator<SignInProfessionalWithEmailCommand>
{
    public SignInProfessionalWithEmailValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
