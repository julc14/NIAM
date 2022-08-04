using AutoMapper;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.UseCases.Exoplanets;

[AutoMap(typeof(Exoplanet))]
public class ExoplanetDto
{
    public int Distance { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DistanceUnits { get; set; } = string.Empty;
    public string ProvidedName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
}
