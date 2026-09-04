using FluentValidation;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Infrastructure.Configuration;

namespace RaphCare.Infrastructure.Storage;

/// <summary>Stores patient profile photos in <see cref="IObjectStorage"/>.</summary>
public sealed class PatientProfilePhotoStorage(
    IObjectStorage objectStorage,
    IOptions<AzureStorageOptions> storageOptions,
    IOptions<PatientProfilePhotoOptions> photoOptions) : IPatientProfilePhotoStorage
{
    private readonly IObjectStorage _objectStorage = objectStorage;
    private readonly AzureStorageOptions _storageOptions = storageOptions.Value;
    private readonly PatientProfilePhotoOptions _photoOptions = photoOptions.Value;

    public async Task<PatientProfilePhotoSaveResult> SaveAsync(
        Guid patientId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (patientId == Guid.Empty)
            throw new ValidationException("Patient id is required.");

        var normalizedType = NormalizeContentType(contentType);
        if (!_photoOptions.AllowedContentTypes.Contains(normalizedType, StringComparer.OrdinalIgnoreCase))
            throw new ValidationException("Unsupported image type. Use JPEG, PNG, or WebP.");

        await using var buffer = await CopyWithLimitAsync(content, _photoOptions.MaxBytes, "Image", cancellationToken)
            .ConfigureAwait(false);

        var extension = ExtensionForContentType(normalizedType);
        var relativePath = $"{patientId:N}{extension}";
        var container = _storageOptions.PhotosContainer;
        await _objectStorage.DeleteByPrefixAsync(container, $"{patientId:N}", cancellationToken).ConfigureAwait(false);
        await _objectStorage.SaveAsync(container, relativePath, buffer, normalizedType, cancellationToken)
            .ConfigureAwait(false);

        return new PatientProfilePhotoSaveResult
        {
            RelativePath = relativePath,
            ContentType = normalizedType
        };
    }

    public async Task<PatientProfilePhotoReadResult?> OpenReadAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        if (patientId == Guid.Empty)
            return null;

        foreach (var extension in new[] { ".jpg", ".jpeg", ".png", ".webp" })
        {
            var stored = await _objectStorage
                .OpenReadAsync(_storageOptions.PhotosContainer, $"{patientId:N}{extension}", cancellationToken)
                .ConfigureAwait(false);
            if (stored is null)
                continue;

            return new PatientProfilePhotoReadResult
            {
                Content = stored.Content,
                ContentType = stored.ContentType
            };
        }

        return null;
    }

    public Task DeleteAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        if (patientId == Guid.Empty)
            return Task.CompletedTask;

        return _objectStorage.DeleteByPrefixAsync(
            _storageOptions.PhotosContainer,
            $"{patientId:N}",
            cancellationToken);
    }

    internal static async Task<MemoryStream> CopyWithLimitAsync(
        Stream content,
        int maxBytes,
        string label,
        CancellationToken cancellationToken)
    {
        var buffer = new MemoryStream();
        var scratch = new byte[81920];
        long total = 0;
        int read;
        while ((read = await content.ReadAsync(scratch.AsMemory(0, scratch.Length), cancellationToken).ConfigureAwait(false)) > 0)
        {
            total += read;
            if (total > maxBytes)
                throw new ValidationException($"{label} must be {maxBytes / (1024 * 1024)} MB or smaller.");

            await buffer.WriteAsync(scratch.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }

        buffer.Position = 0;
        return buffer;
    }

    private static string NormalizeContentType(string contentType)
    {
        var value = contentType.Split(';', 2)[0].Trim().ToLowerInvariant();
        return value switch
        {
            "image/jpg" => "image/jpeg",
            _ => value
        };
    }

    private static string ExtensionForContentType(string contentType) =>
        contentType switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
}
