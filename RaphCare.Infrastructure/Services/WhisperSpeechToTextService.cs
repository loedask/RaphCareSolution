using System.Text;
using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;
using Whisper.net;

namespace RaphCare.Infrastructure.Services;

/// <summary>Local speech-to-text via Whisper.net (default provider).</summary>
public sealed partial class WhisperSpeechToTextService(
    WhisperModelHolder modelHolder,
    ILogger<WhisperSpeechToTextService> logger) : ISpeechToTextService
{
    public async Task<TranscriptionResult> TranscribeAsync(Stream audioStream, string language, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(audioStream);

        await using var wavStream = await WavStreamHelper.MaterializeWavAsync(audioStream, cancellationToken).ConfigureAwait(false);
        var factory = await modelHolder.GetFactoryAsync(cancellationToken).ConfigureAwait(false);

        var builder = factory.CreateBuilder();
        var whisperLanguage = MapWhisperLanguage(language);
        if (string.Equals(whisperLanguage, "auto", StringComparison.OrdinalIgnoreCase))
            builder.WithLanguage("auto");
        else
            builder.WithLanguage(whisperLanguage);

        using var processor = builder.Build();

        var transcript = new StringBuilder();
        await foreach (var segment in processor.ProcessAsync(wavStream, cancellationToken).ConfigureAwait(false))
        {
            if (!string.IsNullOrWhiteSpace(segment.Text))
                transcript.Append(segment.Text.Trim()).Append(' ');
        }

        var fullText = transcript.ToString().Trim();
        LogWhisperTranscriptionCompleted(fullText.Length);

        return new TranscriptionResult
        {
            FullText = fullText,
            ExtractedFields = VoiceTranscriptionFieldExtractor.Extract(fullText),
        };
    }

    private static string MapWhisperLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return "auto";

        var primary = language.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(primary) ? "auto" : primary.ToLowerInvariant();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Whisper transcription completed ({Length} chars).")]
    private partial void LogWhisperTranscriptionCompleted(int length);
}
