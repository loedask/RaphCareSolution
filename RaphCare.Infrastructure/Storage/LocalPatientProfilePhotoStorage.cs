using FluentValidation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Infrastructure.Configuration;

namespace RaphCare.Infrastructure.Storage;

/// <summary>Stores patient profile photos on the API host under <see cref="PatientProfilePhotoOptions.StorageSubdirectory"/>.</summary>
public sealed class LocalPatientProfilePhotoStorage(
    IHostEnvironment hostEnvironment,
    IOptions<PatientProfilePhotoOptions> options) : IPatientProfilePhotoStorage
{
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly PatientProfilePhotoOptions _options = options.Value;

    public async Task<PatientProfilePhotoSaveResult> SaveAsync(
        Guid patientId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (patientId == Guid.Empty)
            throw new ValidationException("Patient id is required.");

        var normalizedType = NormalizeContentType(contentType);
        if (!_options.AllowedContentTypes.Contains(normalizedType, StringComparer.OrdinalIgnoreCase))
            throw new ValidationException("Unsupported image type. Use JPEG, PNG, or WebP.");

        var extension = ExtensionForContentType(normalizedType);
        var relativePath = $"{patientId:N}{extension}";
        var fullPath = Path.Combine(GetStorageRoot(), relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var file = File.Create(fullPath);
        var buffer = new byte[81920];
        int read;
        long total = 0;
        while ((read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) > 0)
        {
            total += read;
            if (total > _options.MaxBytes)
                throw new ValidationException($"Image must be {_options.MaxBytes / (1024 * 1024)} MB or smaller.");

            await file.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }

        return new PatientProfilePhotoSaveResult
        {
            RelativePath = relativePath,
            ContentType = normalizedType
        };
    }

    public Task<PatientProfilePhotoReadResult?> OpenReadAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        if (patientId == Guid.Empty)
            return Task.FromResult<PatientProfilePhotoReadResult?>(null);

        var root = GetStorageRoot();
        var matches = Directory.Exists(root)
            ? Directory.GetFiles(root, $"{patientId:N}.*")
            : Array.Empty<string>();

        var path = matches.FirstOrDefault();
        if (path is null)
            return Task.FromResult<PatientProfilePhotoReadResult?>(null);

        Stream stream = File.OpenRead(path);
        var contentType = ContentTypeForExtension(Path.GetExtension(path));
        return Task.FromResult<PatientProfilePhotoReadResult?>(new PatientProfilePhotoReadResult
        {
            Content = stream,
            ContentType = contentType
        });
    }

    public Task DeleteAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        if (patientId == Guid.Empty)
            return Task.CompletedTask;

        var root = GetStorageRoot();
        if (!Directory.Exists(root))
            return Task.CompletedTask;

        foreach (var path in Directory.GetFiles(root, $"{patientId:N}.*"))
            File.Delete(path);

        return Task.CompletedTask;
    }

    private string GetStorageRoot() =>
        Path.Combine(_hostEnvironment.ContentRootPath, _options.StorageSubdirectory.Replace('/', Path.DirectorySeparatorChar));

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

    private static string ContentTypeForExtension(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
}
