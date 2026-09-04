namespace RaphCare.Application.Common.Interfaces;

/// <summary>Stores voice-onboarding audio privately and returns a durable storage key.</summary>
public interface IVoiceRecordingStorage
{
    Task<VoiceRecordingSaveResult> SaveAsync(
        Guid patientId,
        Guid recordingId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);
}

/// <summary>Result of saving a voice onboarding recording.</summary>
public sealed class VoiceRecordingSaveResult
{
    public required string StorageKey { get; init; }
    public int DurationSeconds { get; init; }
}
