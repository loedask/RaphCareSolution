using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Routes <see cref="ISpeechToTextService"/> to Whisper or Azure based on <see cref="SpeechToTextOptions.Provider"/>.</summary>
public sealed class SpeechToTextRouter(
    IOptionsMonitor<SpeechToTextOptions> options,
    WhisperSpeechToTextService whisper,
    AzureSpeechToTextService azure) : ISpeechToTextService
{
    public Task<TranscriptionResult> TranscribeAsync(Stream audioStream, string language, CancellationToken cancellationToken = default) =>
        options.CurrentValue.Provider == SpeechToTextProvider.Azure
            ? azure.TranscribeAsync(audioStream, language, cancellationToken)
            : whisper.TranscribeAsync(audioStream, language, cancellationToken);
}
