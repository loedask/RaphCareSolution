namespace RaphCare.Application.Features.PatientTelehealth.DTOs;

/// <summary>Credentials for joining an Agora RTC channel (native SDK or custom UI).</summary>
public class TelehealthJoinInfoDto
{
    public Guid TeleSessionId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public uint Uid { get; set; }
    public string? AppId { get; set; }
    public string? RtcToken { get; set; }
    public long TokenExpiresAtUnix { get; set; }
    public bool RtcConfigured { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
}
