using NameItAfterMe.Application.Infrastructure.Files;
using System.Diagnostics.CodeAnalysis;

namespace NameItAfterMe.Application.UseCases.PictureOfTheDay;

public class PictureOfTheDayImageHandler : IImageHandler
{
    private readonly IImageHandler _baseHandler;

    public PictureOfTheDayImageHandler(IImageHandler baseHandler)
        => _baseHandler = baseHandler;

    ///<inheritdoc />
    public Task<ImageMetadata> SaveAsync(
        string name,
        Func<Task<Stream>> getStreamContent)
            => _baseHandler.SaveAsync(name, getStreamContent);

    ///<inheritdoc />
    ///<remarks>
    ///     This decorator will only return a true search result if the image was downloaded today.
    /// </remarks>
    public bool TrySearch(string fileName, [MaybeNullWhen(false)] out ImageMetadata image)
    {
        var result = _baseHandler.TrySearch(fileName, out image);

        if (!result)
        {
            // if false, nothing else to check, return false.
            return result;
        }

        // if the download day != today, return false
        // even though image is there we dont want it since its not the 'image of the day'
        if (DateTime.UtcNow.Day != image!.DownloadDate.Day)
        {
            image = null;
            return false;
        }
        
        // todo some way to delete old images.

        return true;
    }
}
