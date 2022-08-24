using NameItAfterMe.Application.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace NameItAfterMe.Application.Infrastructure.Files;

public interface IImageHandler<T>
    where T : IImage, new()
{
    /// <summary>
    ///     Enumerates the images,
    /// </summary>
    /// <param name="token">
    ///     The cancellation token.
    /// </param>
    /// <returns>
    ///     A stream of images.
    /// </returns>
    IAsyncEnumerable<T> EnumerateImagesAsync(CancellationToken token = default);

    /// <summary>
    ///     Uploads the provided stream as an image.
    /// </summary>
    /// <param name="stream">
    ///     The content stream.
    /// </param>
    /// <param name="name">
    ///     The name to give the image, or a random guid if not provided.
    /// </param>
    /// <param name="token">
    ///     The cancellation token.
    /// </param>
    /// <returns>
    ///     The uploaded image metadata.
    /// </returns>
    Task<T> UploadAsync(Stream stream, string? name = null, CancellationToken token = default);
}