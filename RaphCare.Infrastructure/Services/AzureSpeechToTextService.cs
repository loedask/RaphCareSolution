using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Azure Cognitive Services speech-to-text (optional provider).</summary>
public sealed class AzureSpeechToTextService(
    IOptionsMonitor<SpeechToTextOptions> options,
    ILogger<AzureSpeechToTextService> logger) : ISpeechToTextService
{
    public async Task<TranscriptionResult> TranscribeAsync(Stream audioStream, string language, CancellationToken cancellationToken = default)
    {
        var azure = options.CurrentValue.Azure;
        if (!azure.IsEnabled)
        {
            throw new InvalidOperationException(
                "SpeechToText provider is Azure but SpeechToText:Azure:SubscriptionKey and Region are not configured.");
        }

        await using var wavStream = await WavStreamHelper.MaterializeWavAsync(audioStream, cancellationToken).ConfigureAwait(false);
        var tempPath = Path.Combine(Path.GetTempPath(), $"raphcare-voice-{Guid.NewGuid():N}.wav");
        await using (var file = File.Create(tempPath))
        {
            await wavStream.CopyToAsync(file, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            var speechConfig = SpeechConfig.FromSubscription(azure.SubscriptionKey!, azure.Region!);
            speechConfig.SpeechRecognitionLanguage = MapAzureLanguage(language);

            using var audioConfig = AudioConfig.FromWavFileInput(tempPath);
            using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

            cancellationToken.ThrowIfCancellationRequested();
            var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

            if (result.Reason == ResultReason.Canceled)
            {
                var details = CancellationDetails.FromResult(result);
                throw new InvalidOperationException($"Azure Speech canceled: {details.Reason} — {details.ErrorDetails}");
            }

            if (result.Reason != ResultReason.RecognizedSpeech)
                throw new InvalidOperationException($"Azure Speech failed with reason {result.Reason}.");

            var fullText = result.Text?.Trim() ?? string.Empty;
            logger.LogInformation("Azure Speech transcription completed ({Length} chars).", fullText.Length);

            return new TranscriptionResult
            {
                FullText = fullText,
                ExtractedFields = VoiceTranscriptionFieldExtractor.Extract(fullText),
            };
        }
        finally
        {
            try { File.Delete(tempPath); }
            catch (Exception ex) { logger.LogDebug(ex, "Failed to delete temp wav {Path}.", tempPath); }
        }
    }

    private static string MapAzureLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return "en-US";

        return language.Contains('-', StringComparison.Ordinal) ? language : $"{language}-US";
    }
}
