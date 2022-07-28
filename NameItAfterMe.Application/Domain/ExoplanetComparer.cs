using System.Diagnostics.CodeAnalysis;

namespace NameItAfterMe.Application.Domain;

public class ExoplanetComparer : IEqualityComparer<Exoplanet>
{
    public bool Equals(Exoplanet? x, Exoplanet? y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode([DisallowNull] Exoplanet obj) => obj.Name.GetHashCode();
}
