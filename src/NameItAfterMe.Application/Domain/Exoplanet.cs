namespace NameItAfterMe.Application.Domain;

public class Exoplanet
{
    public string Id { get; set; } = null!;
    public required string Name { get; init; }
    public required string HostName { get; init; }
    public required Distance Distance { get; init; }
    public required string ImageUrl { get; init; }
    public string? ProvidedName { get; set; }
    public string? Story { get; set; }

    public void NameIt(string providedName, string story)
    {
        ProvidedName = providedName;
        Story = story;
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