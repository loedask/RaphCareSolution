using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Outbound SMS placeholder when Twilio is not fully configured (<see cref="TwilioSmsOptions.IsEnabled"/> is false). Use <see cref="TwilioSmsService"/> in environments with valid Twilio settings.</summary>
public class SmsService(ILogger<SmsService> logger, IHostEnvironment hostEnvironment) : ISmsService
{
    private readonly ILogger<SmsService> _logger = logger;
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;

    public Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SMS placeholder: To={PhoneNumber}, Length={Length}", phoneNumber, message.Length);

        // No real SMS in this implementation; surface the OTP in logs during local/dev runs (console + debug).
        if (_hostEnvironment.IsDevelopment())
        {
            _logger.LogWarning(
                "[Development] SMS not sent. OTP for testing — Phone={PhoneNumber} | {Message}",
                phoneNumber,
                message);
        }

        // TODO: Integrate with Twilio, Africa's Talking, or similar
        return Task.CompletedTask;
    }
}
