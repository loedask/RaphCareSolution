using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Outbound SMS integration. Placeholder for Twilio, Africa's Talking, or similar.</summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SMS placeholder: To={PhoneNumber}, Length={Length}", phoneNumber, message.Length);
        // TODO: Integrate with Twilio, Africa's Talking, or similar
        return Task.CompletedTask;
    }
}
