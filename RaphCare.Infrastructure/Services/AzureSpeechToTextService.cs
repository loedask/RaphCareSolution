using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>
/// Placeholder speech-to-text implementation. Replace with Azure Speech SDK integration later.
/// </summary>
public class AzureSpeechToTextService : ISpeechToTextService
{
    public async Task<TranscriptionResult> TranscribeAsync(Stream audioStream, string language, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // Consume stream in real implementation
        _ = language;

        return new TranscriptionResult
        {
            FullText = "My name is John Doe, I was born on 15 March 1988.",
            ExtractedFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["FirstName"] = "John",
                ["LastName"] = "Doe",
                ["DateOfBirth"] = "1988-03-15"
            }
        };
    }
}
