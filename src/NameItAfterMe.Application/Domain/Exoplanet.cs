namespace NameItAfterMe.Application.Domain;

public class Exoplanet
{
    public string Id { get; set; } = null!;
    public string Name { get; private set; }
    public string? ProvidedName { get; private set; }
    public string HostName { get; private set; }
    public Distance Distance { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Exoplanet() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Exoplanet(Distance distance, string hostName, string name)
    {
        Distance = distance;
        HostName = hostName;
        Name = name;
    }

    public void NameIt(string name)
    {
        ProvidedName = name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Exoplanet other)
        {
            return other.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public override int GetHashCode() => Name.GetHashCode();
}