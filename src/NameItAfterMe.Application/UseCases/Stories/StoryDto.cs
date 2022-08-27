namespace NameItAfterMe.Application.UseCases.Stories;

public record StoryDto
{
    public required string Name { get; init; }
    public required IEnumerable<string> WordDescriptor { get; init; }
}
