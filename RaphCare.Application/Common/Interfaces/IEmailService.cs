namespace RaphCare.Application.Common.Interfaces;

/// <summary>Sends outbound email (verification codes, invitations, and support notices).</summary>
public interface IEmailService
{
    Task SendEmailAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default);

    /// <summary>Sends a multipart message with a plain-text body and an HTML body.</summary>
    Task SendEmailAsync(
        string recipient,
        string subject,
        string plainBody,
        string htmlBody,
        CancellationToken cancellationToken = default);
}

