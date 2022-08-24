namespace NameItAfterMe.Application.Domain;

public class PictureOfTheDay
{
    public required string Title { get; set; }
    public required string Url { get; set; } = string.Empty;
    public Stream? Content { get; set; }
    public string? MediaType { get; set; }
}