using AutoMapper;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.UseCases.Exoplanets.GetExoplanets;

public class ExoplanetDto
{
    public string Distance { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DistanceUnits { get; set; } = string.Empty;
    public string ProvidedName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string ImageUrl { get; set; }
}
