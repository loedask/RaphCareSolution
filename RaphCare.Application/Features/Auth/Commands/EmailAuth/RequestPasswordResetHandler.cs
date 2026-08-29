using MediatR;
using RaphCare.Application.Common.Email;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Identity;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class RequestPasswordResetHandler(
    IEmailPasswordAuthService emailPasswordAuth,
    IEmailOtpService emailOtpService,
    IEmailService emailService) : IRequestHandler<RequestPasswordResetCommand>
{
    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var hasCredential = await emailPasswordAuth
            .HasEmailPasswordCredentialAsync(email, cancellationToken)
            .ConfigureAwait(false);

        // Do not reveal whether the email exists.
        if (!hasCredential)
            return;

        // Demo pack addresses are not real mailboxes. Partners use the shared Demo:Password instead.
        if (DemoPackAccounts.IsDemoEmail(email))
            return;

        var code = await emailOtpService.GenerateOtpAsync(email, cancellationToken).ConfigureAwait(false);
        var content = PasswordResetEmail.Create(code);
        await emailService
            .SendEmailAsync(email, content.Subject, content.PlainBody, content.HtmlBody, cancellationToken)
            .ConfigureAwait(false);
    }
}
