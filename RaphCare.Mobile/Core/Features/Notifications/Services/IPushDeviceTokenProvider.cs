namespace RaphCare.Mobile.Core.Features.Notifications.Services;

/// <summary>Supplies a device push token (FCM/APNs when wired; stable local token for demos).</summary>
public interface IPushDeviceTokenProvider
{
    Task<PushDeviceToken?> GetTokenAsync(CancellationToken cancellationToken = default);
}

/// <summary>Platform push token payload for API registration.</summary>
public sealed record PushDeviceToken(string Token, string Platform);
