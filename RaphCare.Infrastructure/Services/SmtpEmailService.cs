using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Sends email via SMTP (e.g. mail.yindula.com:465 for raphcare@yindula.com).</summary>
public sealed partial class SmtpEmailService(
    IOptionsMonitor<SmtpOptions> options,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    public Task SendEmailAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default) =>
        SendCoreAsync(recipient, subject, body, htmlBody: null, cancellationToken);

    public Task SendEmailAsync(
        string recipient,
        string subject,
        string plainBody,
        string htmlBody,
        CancellationToken cancellationToken = default) =>
        SendCoreAsync(recipient, subject, plainBody, htmlBody, cancellationToken);

    private async Task SendCoreAsync(
        string recipient,
        string subject,
        string plainBody,
        string? htmlBody,
        CancellationToken cancellationToken)
    {
        var o = options.CurrentValue;
        if (!o.IsEnabled)
        {
            LogSmtpSkippedNotConfigured();
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(o.FromDisplayName, o.FromAddress));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = CreateBody(plainBody, htmlBody);

        using var client = new SmtpClient();
        await client.ConnectAsync(o.Host, o.Port, o.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, cancellationToken)
            .ConfigureAwait(false);
        await client.AuthenticateAsync(o.Username, o.Password, cancellationToken).ConfigureAwait(false);
        await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

        LogSmtpEmailSent(recipient, subject);
    }

    private static MimeEntity CreateBody(string plainBody, string? htmlBody)
    {
        var plain = new TextPart("plain") { Text = plainBody };
        if (string.IsNullOrWhiteSpace(htmlBody))
            return plain;

        var alternative = new Multipart("alternative")
        {
            plain,
            new TextPart("html") { Text = htmlBody }
        };
        return alternative;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "SMTP email skipped: Smtp section is not fully configured.")]
    private partial void LogSmtpSkippedNotConfigured();

    [LoggerMessage(Level = LogLevel.Information, Message = "SMTP email sent to {To}, subject {Subject}.")]
    private partial void LogSmtpEmailSent(string to, string subject);
}
