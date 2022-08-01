using Refit;

namespace NameItAfterMe.Infrastructure;

public interface IExoplanetService
{
    [Get("/TAP/sync?query=select+sy_dist,pl_name,hostname,disc_year,discoverymethod+from+ps&format=json")]
    Task<IEnumerable<ExoplanetQueryResponse>> GetAllPlanets();
}