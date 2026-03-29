namespace RaphCare.Application.Common.Interfaces;

/// <summary>Builds Agora RTC tokens for patient/provider clients. Optional when Agora app certificate is not configured.</summary>
public interface ITelehealthRtcTokenGenerator
{
    bool IsConfigured { get; }
    string? AppId { get; }
    string BuildRtcToken(string channelName, uint uid, int ttlSeconds = 3600);
}
