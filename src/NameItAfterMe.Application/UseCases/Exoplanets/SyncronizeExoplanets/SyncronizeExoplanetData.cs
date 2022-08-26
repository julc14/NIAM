using MediatR;
using Microsoft.EntityFrameworkCore;
using NameItAfterMe.Application.Domain;
using NameItAfterMe.Application.Infrastructure.Files;
using NameItAfterMe.Application.Infrastructure.Nasa.Exoplanet;
using NameItAfterMe.Infrastructure.Persistance;

namespace NameItAfterMe.Application.UseCases.Exoplanets.SyncronizeExoplanets;

public class SyncronizeExoplanetData : IRequest
{
}

public class SyncronizeExoplanetDataHandler : IRequestHandler<SyncronizeExoplanetData>
{
    private readonly IExoplanetService _exoplanetApi;
    private readonly ExoplanetContext _db;
    private readonly IImageHandler<ExoplanetImage> _exoplanetImageHandler;

    public SyncronizeExoplanetDataHandler(
        IImageHandler<ExoplanetImage> exoplanetImageHandler,
        IExoplanetService exoplanetApi,
        ExoplanetContext db)
            => (_exoplanetApi, _db, _exoplanetImageHandler) = (exoplanetApi, db, exoplanetImageHandler);

    public async Task<Unit> Handle(SyncronizeExoplanetData request, CancellationToken cancellationToken)
    {
        // todo: lazy initialization
        var exoplanetImages = await _exoplanetImageHandler
            .EnumerateImagesAsync(cancellationToken)
            .ToListAsync(cancellationToken);

        // let FindAsync below check the locally loaded cache avoiding db roundtrips
        await _db
            .Set<Exoplanet>()
            .LoadAsync(cancellationToken);

        var nasaExoplanets =
            from response in await _exoplanetApi.GetAllExoplanets()
            where response.Distance.HasValue
            where response.HostName is not null
            where response.Name is not null
            let distance = new Distance()
            {
                Unit = "Parsecs",
                Value = response.Distance!.Value
            }
            select (response.Name!, response.HostName!, distance);

        foreach (var (name, hostName, distance) in nasaExoplanets.Distinct())
        {
            var dbitem = await _db.FindAsync<Exoplanet>(name);

            var planet = new Exoplanet(
                distance,
                hostName,
                // since we are randomly assigning each planet an image, only assign if null.
                // otherwise we will update each item to pointlessly assign a different random image
                dbitem?.ImageUrl ?? exoplanetImages.PickRandom().Url)
            {
                Name = name,
            };

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