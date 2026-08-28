namespace RaphCare.Application.Features.Organization.DTOs;

/// <summary>Agora RTC credentials for an admin/provider telehealth join.</summary>
public sealed class AdminClinicTeleJoinInfoDto
{
    public Guid TeleSessionId { get; set; }
    public Guid VisitId { get; set; }
    public Guid AppointmentId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public uint Uid { get; set; }
    public string? AppId { get; set; }
    public string? RtcToken { get; set; }
    public long TokenExpiresAtUnix { get; set; }
    public bool RtcConfigured { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public string? PatientName { get; set; }
    public string? ProviderDisplayName { get; set; }
}
