namespace RaphCare.Application.Common.Interfaces;

/// <summary>Private object store for patient files (Azure Blob in hosted environments, local disk in Development).</summary>
public interface IObjectStorage
{
    Task SaveAsync(
        string container,
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<StoredObjectReadResult?> OpenReadAsync(
        string container,
        string key,
        CancellationToken cancellationToken = default);

    Task DeleteByPrefixAsync(
        string container,
        string keyPrefix,
        CancellationToken cancellationToken = default);
}

/// <summary>Readable object payload from <see cref="IObjectStorage"/>.</summary>
public sealed class StoredObjectReadResult
{
    public required Stream Content { get; init; }
    public required string ContentType { get; init; }
}
