using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Metadata for a voice recording used during patient onboarding.
/// Audio files are stored in blob storage; this entity holds the reference and metadata.
/// </summary>
public class VoiceRecording : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }
    public string StorageUrl { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string Language { get; set; } = string.Empty;
}
