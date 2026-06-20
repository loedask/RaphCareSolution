using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class SendEmailVerificationHandler(
    IEmailOtpService emailOtpService,
    IEmailService emailService) : IRequestHandler<SendEmailVerificationCommand>
{
    public async Task Handle(SendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var code = await emailOtpService.GenerateOtpAsync(request.Email, cancellationToken).ConfigureAwait(false);
        var subject = "Your RaphCare verification code";
        var body = $"Your RaphCare verification code is: {code}\n\nThis code expires in 10 minutes. If you did not request this, you can ignore this email.";
        await emailService.SendEmailAsync(request.Email.Trim(), subject, body, cancellationToken).ConfigureAwait(false);
    }
}
