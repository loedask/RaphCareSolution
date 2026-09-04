using Microsoft.Extensions.Hosting;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Storage;

/// <summary>Development-only disk store under <c>App_Data/object-store</c>.</summary>
public sealed class LocalFileObjectStorage : IObjectStorage
{
    private readonly string _root;

    public LocalFileObjectStorage(IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);
        if (!hostEnvironment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "AzureStorage:ConnectionString is required outside Development. Run scripts/New-RaphCareAzureBlobStorage.ps1.");
        }

        _root = Path.Combine(hostEnvironment.ContentRootPath, "App_Data", "object-store");
    }

    public async Task SaveAsync(
        string container,
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var path = GetObjectPath(container, key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using (var file = File.Create(path))
        {
            await content.CopyToAsync(file, cancellationToken).ConfigureAwait(false);
        }

        await File.WriteAllTextAsync(GetContentTypePath(path), contentType, cancellationToken).ConfigureAwait(false);
    }

    public Task<StoredObjectReadResult?> OpenReadAsync(
        string container,
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = GetObjectPath(container, key);
        if (!File.Exists(path))
            return Task.FromResult<StoredObjectReadResult?>(null);

        var contentTypePath = GetContentTypePath(path);
        var contentType = File.Exists(contentTypePath)
            ? File.ReadAllText(contentTypePath).Trim()
            : ContentTypeForExtension(Path.GetExtension(path));

        return Task.FromResult<StoredObjectReadResult?>(new StoredObjectReadResult
        {
            Content = File.OpenRead(path),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
        });
    }

    public Task DeleteByPrefixAsync(
        string container,
        string keyPrefix,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var containerRoot = Path.Combine(_root, ObjectStorageKey.NormalizeContainer(container));
        if (!Directory.Exists(containerRoot))
            return Task.CompletedTask;

        var prefix = ObjectStorageKey.NormalizeKey(keyPrefix);
        var matches = Directory.GetFiles(containerRoot, "*", SearchOption.AllDirectories)
            .Where(path =>
            {
                var relative = Path.GetRelativePath(containerRoot, path).Replace('\\', '/');
                if (relative.EndsWith(".contenttype", StringComparison.OrdinalIgnoreCase))
                    relative = relative[..^".contenttype".Length];
                return relative.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            })
            .ToList();

        foreach (var path in matches)
            File.Delete(path);

        return Task.CompletedTask;
    }

    private string GetObjectPath(string container, string key) =>
        Path.Combine(_root, ObjectStorageKey.NormalizeContainer(container), ObjectStorageKey.NormalizeKey(key).Replace('/', Path.DirectorySeparatorChar));

    private static string GetContentTypePath(string objectPath) => objectPath + ".contenttype";

    private static string ContentTypeForExtension(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".wav" => "audio/wav",
            ".webm" => "audio/webm",
            ".mp3" => "audio/mpeg",
            ".m4a" => "audio/mp4",
            ".aac" => "audio/aac",
            _ => "application/octet-stream"
        };
}
