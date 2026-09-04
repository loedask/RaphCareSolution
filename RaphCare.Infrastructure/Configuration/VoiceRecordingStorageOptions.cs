namespace RaphCare.Infrastructure.Configuration;

/// <summary>Limits and allowed types for voice onboarding audio.</summary>
public sealed class VoiceRecordingStorageOptions
{
    public const string SectionName = "VoiceRecordings";

    public int MaxBytes { get; set; } = 25 * 1024 * 1024;

    public string[] AllowedContentTypes { get; set; } =
    [
        "audio/wav",
        "audio/wave",
        "audio/x-wav",
        "audio/webm",
        "audio/mpeg",
        "audio/mp4",
        "audio/aac"
    ];
}
