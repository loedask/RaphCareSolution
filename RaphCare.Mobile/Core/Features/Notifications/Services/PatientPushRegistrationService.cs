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
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _lastRegisteredToken;

    public async Task RegisterCurrentDeviceAsync(CancellationToken cancellationToken = default)
    {
        if (!FeatureFlags.NotificationsEnabled)
            return;

        if (!await _auth.IsAuthenticatedAsync(cancellationToken).ConfigureAwait(false))
            return;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
            if (token is null || string.IsNullOrWhiteSpace(token.Token))
                return;

            if (string.Equals(_lastRegisteredToken, token.Token, StringComparison.Ordinal))
                return;

            var response = await _notifications.RegisterPushDeviceAsync(
                new RegisterPatientPushDeviceRequest
                {
                    DeviceToken = token.Token,
                    Platform = token.Platform
                },
                cancellationToken).ConfigureAwait(false);

            if (response.IsSuccess)
                _lastRegisteredToken = token.Token;
        }
        finally
        {
            _gate.Release();
        }
    }
}
