namespace NameItAfterMe.Application.Domain;

public class Exoplanet
{
    public int Id { get; set; }
    public int Distance { get; set; }
    public string DistanceUnits { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}