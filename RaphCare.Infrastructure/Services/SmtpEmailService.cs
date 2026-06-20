using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Sends email via SMTP (e.g. mail.yindula.com:465 for raphcare@yindula.com).</summary>
public sealed class SmtpEmailService(
    IOptionsMonitor<SmtpOptions> options,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendEmailAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        var o = options.CurrentValue;
        if (!o.IsEnabled)
        {
            logger.LogWarning("SMTP email skipped: Smtp section is not fully configured.");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(o.FromDisplayName, o.FromAddress));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(o.Host, o.Port, o.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, cancellationToken)
            .ConfigureAwait(false);
        await client.AuthenticateAsync(o.Username, o.Password, cancellationToken).ConfigureAwait(false);
        await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("SMTP email sent to {To}, subject {Subject}.", recipient, subject);
    }
}
