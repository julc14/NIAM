namespace NameItAfterMe.Application.Abstractions;

public interface IImage
{
    static abstract string ContainerName { get; }

    string Url { get; init; }

    string Name { get; init; }

    DateTimeOffset CreatedOn { get; init; }
}
