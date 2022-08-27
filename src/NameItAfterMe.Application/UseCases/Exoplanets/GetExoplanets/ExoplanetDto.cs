using AutoMapper;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.UseCases.Exoplanets.GetExoplanets;

public class ExoplanetDto
{
    public required string Distance { get; init; }
    public required string Name { get; init; }
    public required string DistanceUnits { get; init; }
    public required string ProvidedName { get; init; }
    public required string HostName { get; init; }
    public required string ImageUrl { get; init; }
}
