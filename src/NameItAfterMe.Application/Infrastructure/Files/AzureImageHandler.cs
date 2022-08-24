using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using NameItAfterMe.Application.Abstractions;
using System.Runtime.CompilerServices;

namespace NameItAfterMe.Application.Infrastructure.Files;

internal class AzureImageHandler<T> : IImageHandler<T>
    where T : IImage, new()
{
    private readonly Lazy<BlobContainerClient> _container;

    public AzureImageHandler(IOptions<AzureImageHandlerOptions> options)
    {
        _container = new(() => new BlobContainerClient(options.Value.ConnectionString, T.ContainerName));
    }

    ///<inheritdoc/>
    public async IAsyncEnumerable<T> EnumerateImagesAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        var container = _container.Value;
        await container.CreateIfNotExistsAsync(cancellationToken: token);

        await foreach (var blob in container.GetBlobsAsync(cancellationToken: token))
        {
            var url = container.GetBlobClient(blob.Name).Uri.AbsoluteUri;

            yield return new T()
            {
                Url = url,
                Name = blob.Name,
                CreatedOn = blob.Properties.CreatedOn.GetValueOrDefault(),
            };
        }
    }

    ///<inheritdoc/>
    public async Task<T> UploadAsync(Stream stream, string? name = null, CancellationToken token = default)
    {
        var container = _container.Value;
        await container.CreateIfNotExistsAsync(cancellationToken: token);

        name ??= Guid.NewGuid().ToString();
        var client = container.GetBlobClient(name);

        await client.UploadAsync(
            stream,
            new BlobHttpHeaders { ContentType = "image/jpg" },
            cancellationToken: token);

        return new T()
        {
            Url = client.Uri.AbsoluteUri,
            Name = name,
            CreatedOn = DateTimeOffset.UtcNow,
        };
    }
}
