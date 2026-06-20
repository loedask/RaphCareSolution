using FluentValidation;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class SendEmailVerificationValidator : AbstractValidator<SendEmailVerificationCommand>
{
    public SendEmailVerificationValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}
