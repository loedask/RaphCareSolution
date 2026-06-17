using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace RaphCare.Infrastructure.Services;

/// <summary>Twilio Programmable SMS for all <see cref="ISmsService"/> use (e.g. OTP, telehealth). Registered when <see cref="TwilioSmsOptions.IsEnabled"/>.</summary>
public sealed class TwilioSmsService(
    IOptionsMonitor<TwilioSmsOptions> options,
    ILogger<TwilioSmsService> logger) : ISmsService
{
    public async Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var o = options.CurrentValue;
        if (!o.IsEnabled)
        {
            logger.LogWarning("Twilio SMS skipped: options not fully configured.");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        TwilioClient.Init(o.AccountSid, o.AuthToken);

        // Twilio .NET SDK CreateAsync overloads may not accept CancellationToken; honor cancel before the network call.
        await MessageResource.CreateAsync(
            body: message,
            from: new PhoneNumber(o.FromPhoneE164!),
            to: new PhoneNumber(phoneNumber)).ConfigureAwait(false);

        logger.LogInformation("Twilio SMS sent to {To} (length {Len}).", phoneNumber, message.Length);
    }
}
