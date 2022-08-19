namespace NameItAfterMe.Application.UseCases.Exoplanets.GetExoplanetCounts;

public record ExoplanetCountDto
{
    public int UnnamedExoplanets { get; init; }
    public int NamedExoplanets { get; init; }
    public int TotalExoplanets => NamedExoplanets + UnnamedExoplanets;
}
