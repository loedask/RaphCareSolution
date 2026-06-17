namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Transcribes audio to text and extracts structured patient fields for voice onboarding.
/// </summary>
public interface ISpeechToTextService
{
    /// <summary>
    /// Transcribes audio and extracts patient-related fields (e.g. FirstName, LastName, DateOfBirth).
    /// </summary>
    Task<TranscriptionResult> TranscribeAsync(Stream audioStream, string language, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of speech-to-text transcription with optional extracted fields for patient creation.
/// </summary>
public class TranscriptionResult
{
    public string FullText { get; set; } = string.Empty;
    public Dictionary<string, string> ExtractedFields { get; set; } = new();
}
