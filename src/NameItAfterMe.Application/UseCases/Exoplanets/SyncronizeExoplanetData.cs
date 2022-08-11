using MediatR;
using Microsoft.EntityFrameworkCore;
using NameItAfterMe.Application.Domain;
using NameItAfterMe.Application.Infrastructure.Nasa.Exoplanet;
using NameItAfterMe.Infrastructure.Persistance;

namespace NameItAfterMe.Application.UseCases.Exoplanets;

public class SyncronizeExoplanetData : IRequest
{
}

public class SyncronizeExoplanetDataHandler : IRequestHandler<SyncronizeExoplanetData>
{
    private readonly IExoplanetService _exoplanetApi;
    private readonly ExoplanetContext _db;

    public SyncronizeExoplanetDataHandler(
        IExoplanetService exoplanetApi,
        ExoplanetContext db)
            => (_exoplanetApi, _db) = (exoplanetApi, db);

    public async Task<Unit> Handle(SyncronizeExoplanetData request, CancellationToken cancellationToken)
    {
        var exoplanets =
            from response in await _exoplanetApi.GetAllExoplanets()
            where response.Distance.HasValue
            where response.HostName is not null
            where response.Name is not null
            let distance = new Distance("Parsecs", response.Distance!.Value)
            select new Exoplanet(distance, response.HostName!, response.Name!);

        await _db.Set<Exoplanet>().LoadAsync(cancellationToken);

        foreach (var planet in exoplanets.Distinct())
        {
            var dbitem = await _db.FindAsync<Exoplanet>(planet.Name);

            if (dbitem is null)
            {
                _db.Add(planet);
            }
            else
            {
                _db.Entry(dbitem).CurrentValues.SetValues(planet);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}