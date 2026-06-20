using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace RaphCare.Infrastructure.Services;

/// <summary>Twilio Programmable SMS for all <see cref="ISmsService"/> use (e.g. OTP, telehealth). Registered when <see cref="TwilioSmsOptions.IsEnabled"/>.</summary>
public sealed partial class TwilioSmsService(
    IOptionsMonitor<TwilioSmsOptions> options,
    ILogger<TwilioSmsService> logger) : ISmsService
{
    public async Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var o = options.CurrentValue;
        if (!o.IsEnabled)
        {
            LogTwilioSkippedNotConfigured();
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        TwilioClient.Init(o.AccountSid, o.AuthToken);

        await MessageResource.CreateAsync(
            body: message,
            from: new PhoneNumber(o.FromPhoneE164!),
            to: new PhoneNumber(phoneNumber)).ConfigureAwait(false);

        LogTwilioSmsSent(phoneNumber, message.Length);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Twilio SMS skipped: options not fully configured.")]
    private partial void LogTwilioSkippedNotConfigured();

    [LoggerMessage(Level = LogLevel.Information, Message = "Twilio SMS sent to {To} (length {Len}).")]
    private partial void LogTwilioSmsSent(string to, int len);
}
