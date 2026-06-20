using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Logs outbound email when SMTP is not configured; surfaces verification codes in Development logs.</summary>
public sealed partial class DevelopmentEmailService(ILogger<DevelopmentEmailService> logger, IHostEnvironment hostEnvironment) : IEmailService
{
    public Task SendEmailAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        LogEmailPlaceholder(recipient, subject);

        if (hostEnvironment.IsDevelopment())
        {
            LogDevelopmentEmailNotSent(recipient, subject, body);
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Email placeholder: To={To}, Subject={Subject}")]
    private partial void LogEmailPlaceholder(string to, string subject);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "[Development] Email not sent via SMTP. For testing — To={To} | Subject={Subject} | {Body}")]
    private partial void LogDevelopmentEmailNotSent(string to, string subject, string body);
}
