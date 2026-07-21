using Microsoft.Maui.Storage;

namespace RaphCare.Mobile.Core.Features.Notifications.Services;

/// <summary>
/// Demo-friendly token provider: persists a stable local token so <c>PUT push-device</c> works without FCM/APNs.
/// Replace with Firebase Messaging / APNs providers when native push SDKs are configured.
/// </summary>
public sealed class LocalDevelopmentPushDeviceTokenProvider : IPushDeviceTokenProvider
{
    private const string PreferenceKey = "raphcare.push.dev_token";

    public Task<PushDeviceToken?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var token = Preferences.Default.Get(PreferenceKey, string.Empty);
        if (string.IsNullOrWhiteSpace(token))
        {
            token = $"dev-{DeviceInfo.Platform}-{Guid.NewGuid():N}";
            Preferences.Default.Set(PreferenceKey, token);
        }

        var platform = DeviceInfo.Platform.ToString().ToLowerInvariant();
        if (platform is "macatalyst" or "maccatalyst")
            platform = "ios";
        if (platform == "winui" || platform == "unknown")
            platform = "android";

        return Task.FromResult<PushDeviceToken?>(new PushDeviceToken(token, platform));
    }
}
