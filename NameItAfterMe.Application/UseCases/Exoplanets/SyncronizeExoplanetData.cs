using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<SyncronizeExoplanetDataHandler> _logger;

    public SyncronizeExoplanetDataHandler(
        IExoplanetService exoplanetApi,
        ExoplanetContext db,
        ILogger<SyncronizeExoplanetDataHandler> logger)
            => (_exoplanetApi, _db, _logger) = (exoplanetApi, db, logger);

    public async Task<Unit> Handle(SyncronizeExoplanetData request, CancellationToken cancellationToken)
    {
        var sourceExoplanets = await _exoplanetApi.GetAllExoplanets();

        _db.UpdateRange(sourceExoplanets);

        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}