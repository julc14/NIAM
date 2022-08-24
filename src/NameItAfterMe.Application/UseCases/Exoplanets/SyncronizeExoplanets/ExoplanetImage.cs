using NameItAfterMe.Application.Abstractions;

namespace NameItAfterMe.Application.UseCases.Exoplanets.SyncronizeExoplanets;

public class ExoplanetImage : IImage
{
    public static string ContainerName => "exoplanets";

    public string Url { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset CreatedOn { get; init; }
}
