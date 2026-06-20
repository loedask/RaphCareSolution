using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Logs outbound email when SMTP is not configured; surfaces verification codes in Development logs.</summary>
public sealed class DevelopmentEmailService(ILogger<DevelopmentEmailService> logger, IHostEnvironment hostEnvironment) : IEmailService
{
    public Task SendEmailAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Email placeholder: To={To}, Subject={Subject}", recipient, subject);

        if (hostEnvironment.IsDevelopment())
        {
            logger.LogWarning(
                "[Development] Email not sent via SMTP. For testing — To={To} | Subject={Subject} | {Body}",
                recipient,
                subject,
                body);
        }

        return Task.CompletedTask;
    }
}
