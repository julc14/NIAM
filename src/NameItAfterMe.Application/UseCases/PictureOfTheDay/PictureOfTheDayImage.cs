using NameItAfterMe.Application.Abstractions;

namespace NameItAfterMe.Application.UseCases.PictureOfTheDay;

public class PictureOfTheDayImage : IImage
{
    public static string ContainerName => "picturesoftheday";

    public string Url { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset CreatedOn { get; init; }

    public bool IsImageCreatedToday => (DateTime.UtcNow - CreatedOn).Hours <= 24;
}
