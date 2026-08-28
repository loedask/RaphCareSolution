using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Notifications;
using RaphCare.Mobile.Core.Common.Services.Auth;
using FeatureFlags = RaphCare.Mobile.Core.Common.Services.FeatureFlags.FeatureFlags;

namespace RaphCare.Mobile.Core.Features.Notifications.Services;

/// <summary>Registers the current device push token with the patient notifications API after auth.</summary>
public interface IPatientPushRegistrationService
{
    Task RegisterCurrentDeviceAsync(CancellationToken cancellationToken = default);
}

public sealed class PatientPushRegistrationService(
    IAuthService auth,
    IPushDeviceTokenProvider tokenProvider,
    IPatientNotificationsService notifications) : IPatientPushRegistrationService
{
    private readonly IAuthService _auth = auth;
    private readonly IPushDeviceTokenProvider _tokenProvider = tokenProvider;
    private readonly IPatientNotificationsService _notifications = notifications;
    private readonly object _sync = new();
    private string? _lastRegisteredToken;
    private bool _inFlight;

    public async Task RegisterCurrentDeviceAsync(CancellationToken cancellationToken = default)
    {
        if (!FeatureFlags.NotificationsEnabled)
            return;

        if (!await _auth.IsAuthenticatedAsync(cancellationToken).ConfigureAwait(false))
            return;

        lock (_sync)
        {
            if (_inFlight)
                return;
            _inFlight = true;
        }

        try
        {
            var token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
            if (token is null || string.IsNullOrWhiteSpace(token.Token))
                return;

            lock (_sync)
            {
                if (string.Equals(_lastRegisteredToken, token.Token, StringComparison.Ordinal))
                    return;
            }

            var response = await _notifications.RegisterPushDeviceAsync(
                new RegisterPatientPushDeviceRequest
                {
                    DeviceToken = token.Token,
                    Platform = token.Platform
                },
                cancellationToken).ConfigureAwait(false);

            if (response.IsSuccess)
            {
                lock (_sync)
                    _lastRegisteredToken = token.Token;
            }
        }
        finally
        {
            lock (_sync)
                _inFlight = false;
        }
    }
}
