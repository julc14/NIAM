﻿namespace NameItAfterMe.Application.Domain;

public class Exoplanet
{
    public string Id { get; set; } = null!;
    public int Distance { get; set; }
    public string DistanceUnits { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ProvidedName { get; set; }
    public string? HostName { get; set; }

    public bool IsNamed() => HostName is not null;
}