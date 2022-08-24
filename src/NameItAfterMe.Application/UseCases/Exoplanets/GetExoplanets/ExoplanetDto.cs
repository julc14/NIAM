using AutoMapper;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.UseCases.Exoplanets.GetExoplanets;

public class ExoplanetDto
{
    public required string Distance { get; set; }
    public required string Name { get; set; }
    public required string DistanceUnits { get; set; }
    public required string ProvidedName { get; set; }
    public required string HostName { get; set; }
    public required string ImageUrl { get; set; }
}
