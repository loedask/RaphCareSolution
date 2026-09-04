using FluentValidation;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Infrastructure.Configuration;

namespace RaphCare.Infrastructure.Storage;

/// <summary>Stores voice onboarding audio in <see cref="IObjectStorage"/>.</summary>
public sealed class VoiceRecordingStorage(
    IObjectStorage objectStorage,
    IOptions<AzureStorageOptions> storageOptions,
    IOptions<VoiceRecordingStorageOptions> voiceOptions) : IVoiceRecordingStorage
{
    private readonly IObjectStorage _objectStorage = objectStorage;
    private readonly AzureStorageOptions _storageOptions = storageOptions.Value;
    private readonly VoiceRecordingStorageOptions _voiceOptions = voiceOptions.Value;

    public async Task<VoiceRecordingSaveResult> SaveAsync(
        Guid patientId,
        Guid recordingId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (patientId == Guid.Empty)
            throw new ValidationException("Patient id is required.");
        if (recordingId == Guid.Empty)
            throw new ValidationException("Recording id is required.");

        var normalizedType = NormalizeContentType(contentType);
        if (!_voiceOptions.AllowedContentTypes.Contains(normalizedType, StringComparer.OrdinalIgnoreCase))
            throw new ValidationException("Unsupported audio type.");

        await using var buffer = await PatientProfilePhotoStorage
            .CopyWithLimitAsync(content, _voiceOptions.MaxBytes, "Audio", cancellationToken)
            .ConfigureAwait(false);

        var durationSeconds = WavDuration.TryGetSeconds(buffer);
        buffer.Position = 0;

        var extension = ExtensionForContentType(normalizedType);
        var key = $"{patientId:N}/{recordingId:N}{extension}";
        await _objectStorage.SaveAsync(
                _storageOptions.VoiceRecordingsContainer,
                key,
                buffer,
                normalizedType,
                cancellationToken)
            .ConfigureAwait(false);

        return new VoiceRecordingSaveResult
        {
            StorageKey = $"{_storageOptions.VoiceRecordingsContainer}/{key}",
            DurationSeconds = durationSeconds
        };
    }

    private static string NormalizeContentType(string contentType)
    {
        var value = contentType.Split(';', 2)[0].Trim().ToLowerInvariant();
        return value switch
        {
            "audio/wave" => "audio/wav",
            "audio/x-wav" => "audio/wav",
            _ => value
        };
    }

    private static string ExtensionForContentType(string contentType) =>
        contentType switch
        {
            "audio/webm" => ".webm",
            "audio/mpeg" => ".mp3",
            "audio/mp4" => ".m4a",
            "audio/aac" => ".aac",
            _ => ".wav"
        };
}
