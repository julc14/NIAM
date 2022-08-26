using NameItAfterMe.Application.Abstractions;

namespace NameItAfterMe.Application.UseCases.PictureOfTheDay;

public class PictureOfTheDayImage : IImage, IImageRepositoryConfiguration
{
    public static string ContainerName => "picturesoftheday";

    public string Url { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset CreatedOn { get; init; }
}
