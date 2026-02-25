using RaphCare.Domain.Common;

namespace RaphCare.Domain.Telemedicine;

/// <summary>
/// Recording metadata for a telemedicine session.
/// </summary>
public class TeleSessionRecording : BaseEntity
{
    public Guid TeleSessionId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime RecordedAt { get; set; }
    public bool IsEncrypted { get; set; }

    public TeleSession TeleSession { get; set; } = null!;
}
