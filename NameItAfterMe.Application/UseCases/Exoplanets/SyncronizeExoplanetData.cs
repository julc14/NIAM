using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NameItAfterMe.Application.Abstractions;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.UseCases.Exoplanets;

public class SyncronizeExoplanetData : IRequest
{
}

public class SyncronizeExoplanetDataHandler : IRequestHandler<SyncronizeExoplanetData>
{
    private readonly IExoplanetApi _exoplanetApi;
    private readonly IExoplanetContext _db;
    private readonly ILogger<SyncronizeExoplanetDataHandler> _logger;

    public SyncronizeExoplanetDataHandler(
        IExoplanetApi exoplanetApi,
        IExoplanetContext db,
        ILogger<SyncronizeExoplanetDataHandler> logger)
            => (_exoplanetApi, _db, _logger) = (exoplanetApi, db, logger);

    public async Task<Unit> Handle(SyncronizeExoplanetData request, CancellationToken cancellationToken)
    {
        var sourceExoplanets = await _exoplanetApi.GetAllExoplanets();
        var exoplanets = _db.Set<Exoplanet>();

        var newPlanets = sourceExoplanets.Except(
            await exoplanets.ToListAsync(cancellationToken),
            new ExoplanetComparer());

        await exoplanets.AddRangeAsync(newPlanets.ToList(), cancellationToken);

        _logger.LogInformation("Writing {PlanetsToAdd} new planetes to db", newPlanets.Count());
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}