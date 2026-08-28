using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

public sealed class SendEmailVerificationHandler(
    IEmailOtpService emailOtpService,
    IEmailService emailService) : IRequestHandler<SendEmailVerificationCommand>
{
    public Task Handle(SendEmailVerificationCommand request, CancellationToken cancellationToken) =>
        EmailVerificationHelper.SendVerificationEmailAsync(
            request.Email, emailOtpService, emailService, cancellationToken);
}
