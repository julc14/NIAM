namespace NameItAfterMe.Application.Abstractions;

public interface IImage
{
    string Url { get; init; }
    string Name { get; init; }
    DateTimeOffset CreatedOn { get; init; }
}
