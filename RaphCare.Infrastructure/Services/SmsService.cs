using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Outbound SMS placeholder when Twilio is not fully configured (<see cref="TwilioSmsOptions.IsEnabled"/> is false). Use <see cref="TwilioSmsService"/> in environments with valid Twilio settings.</summary>
public sealed partial class SmsService(ILogger<SmsService> logger, IHostEnvironment hostEnvironment) : ISmsService
{
    public Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        LogSmsPlaceholder(phoneNumber, message.Length);

        if (hostEnvironment.IsDevelopment())
        {
            LogDevelopmentSmsNotSent(phoneNumber, message);
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "SMS placeholder: To={PhoneNumber}, Length={Length}")]
    private partial void LogSmsPlaceholder(string phoneNumber, int length);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "[Development] SMS not sent. OTP for testing — Phone={PhoneNumber} | {Message}")]
    private partial void LogDevelopmentSmsNotSent(string phoneNumber, string message);
}
