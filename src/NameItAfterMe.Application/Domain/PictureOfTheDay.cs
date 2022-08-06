namespace NameItAfterMe.Application.Domain;

public class PictureOfTheDay
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Stream? Content { get; set; }
    public string? MediaType { get; set; }
}