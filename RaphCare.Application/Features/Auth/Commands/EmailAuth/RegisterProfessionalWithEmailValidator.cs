using FluentValidation;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class RegisterProfessionalWithEmailValidator : AbstractValidator<RegisterProfessionalWithEmailCommand>
{
    public RegisterProfessionalWithEmailValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
