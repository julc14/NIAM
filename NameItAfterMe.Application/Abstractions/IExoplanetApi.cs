using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.Abstractions;

public interface IExoplanetApi
{
    public Task<IEnumerable<Exoplanet>> GetAllExoplanets();
}
