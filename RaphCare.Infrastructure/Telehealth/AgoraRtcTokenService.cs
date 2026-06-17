using AgoraIO.Media;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Infrastructure.Configuration;

namespace RaphCare.Infrastructure.Telehealth;

/// <summary>Builds RTC tokens using Agora open-source token utilities (NuGet AgoraDynamicKey / AgoraIO).</summary>
public sealed class AgoraRtcTokenService(IOptionsMonitor<AgoraRtcOptions> optionsMonitor) : ITelehealthRtcTokenGenerator
{
    public bool IsConfigured
    {
        get
        {
            var o = optionsMonitor.CurrentValue;
            return !string.IsNullOrWhiteSpace(o.AppId) && !string.IsNullOrWhiteSpace(o.AppCertificate);
        }
    }

    public string? AppId
    {
        get
        {
            var id = optionsMonitor.CurrentValue.AppId;
            return string.IsNullOrWhiteSpace(id) ? null : id.Trim();
        }
    }

    public string BuildRtcToken(string channelName, uint uid, int ttlSeconds = 3600)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Agora RTC is not configured (AgoraRtc:AppId and AppCertificate).");

        var o = optionsMonitor.CurrentValue;
        var expireAt = (uint)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Math.Clamp(ttlSeconds, 60, 86400));
        return RtcTokenBuilder.buildTokenWithUID(
            o.AppId!.Trim(),
            o.AppCertificate!.Trim(),
            channelName,
            uid,
            RtcTokenBuilder.Role.RolePublisher,
            expireAt);
    }
}
