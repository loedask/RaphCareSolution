namespace RaphCare.Infrastructure.Configuration;

/// <summary>Agora Video / RTC (primary real-time media). App certificate must stay server-side only.</summary>
public class AgoraRtcOptions
{
    public const string SectionName = "AgoraRtc";

    public string? AppId { get; set; }
    public string? AppCertificate { get; set; }
}
