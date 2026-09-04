using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Infrastructure.Configuration;

namespace RaphCare.Infrastructure.Storage;

/// <summary>Private Azure Blob store. Containers are not publicly readable.</summary>
public sealed class AzureBlobObjectStorage : IObjectStorage
{
    private readonly BlobServiceClient _blobs;

    public AzureBlobObjectStorage(IOptions<AzureStorageOptions> options)
    {
        var connectionString = options.Value.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("AzureStorage:ConnectionString is required for Azure blob storage.");

        _blobs = new BlobServiceClient(connectionString);
    }

    public async Task SaveAsync(
        string container,
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerAsync(container, cancellationToken).ConfigureAwait(false);
        var blob = containerClient.GetBlobClient(ObjectStorageKey.NormalizeKey(key));
        await blob.UploadAsync(
                content,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<StoredObjectReadResult?> OpenReadAsync(
        string container,
        string key,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerAsync(container, cancellationToken).ConfigureAwait(false);
        var blob = containerClient.GetBlobClient(ObjectStorageKey.NormalizeKey(key));
        try
        {
            var download = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var contentType = download.Value.Details.ContentType;
            return new StoredObjectReadResult
            {
                Content = download.Value.Content,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task DeleteByPrefixAsync(
        string container,
        string keyPrefix,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerAsync(container, cancellationToken).ConfigureAwait(false);
        var prefix = ObjectStorageKey.NormalizeKey(keyPrefix);
        await foreach (var item in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken)
                           .ConfigureAwait(false))
        {
            await containerClient.DeleteBlobIfExistsAsync(item.Name, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task<BlobContainerClient> GetContainerAsync(string container, CancellationToken cancellationToken)
    {
        var client = _blobs.GetBlobContainerClient(ObjectStorageKey.NormalizeContainer(container));
        await client.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return client;
    }
}
