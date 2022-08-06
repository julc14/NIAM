using System.Diagnostics.CodeAnalysis;

namespace NameItAfterMe.Application.Infrastructure.Files;

public interface IImageHandler
{
    /// <summary>
    ///     The local folder to search. Defaults to 'Images'.
    /// </summary>
    string LocalFolder { get; set; }

    /// <summary>
    ///     Persist the content stream to some storage mechanism.
    /// </summary>
    /// <param name="fileIdentifier">
    ///     The desired file identifier.
    /// </param>
    /// <param name="getStreamContent">
    ///     How to aquire the desired content to save.
    /// </param>
    /// <returns>
    ///     Result describing the saved image.
    /// </returns>
    Task<ImageMetadata> SaveAsync(string fileIdentifier, Func<Task<Stream>> getStreamContent);

    /// <summary>
    ///     Search for a file matching the given identifer.
    /// </summary>
    /// <param name="fileIdentifier">
    ///     The file identifier.
    /// </param>
    /// <param name="image">
    ///     The found image, if any.
    /// </param>
    /// <returns>
    ///     Whether the search was succesfull.
    /// </returns>
    bool TrySearch(string fileIdentifier, [MaybeNullWhen(false)] out ImageMetadata image);
}
