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
    private readonly IImageHandler _imageHandler;

    public SyncronizeExoplanetDataHandler(
        IImageHandler imageHandler,
        IExoplanetService exoplanetApi,
        ExoplanetContext db)
            => (_exoplanetApi, _db, _imageHandler) = (exoplanetApi, db, imageHandler);

    public async Task<Unit> Handle(SyncronizeExoplanetData request, CancellationToken cancellationToken)
    {
        if (_imageHandler is StaticImageHandler staticImageHandler)
        {
            staticImageHandler.LocalFolder = "Images\\Exoplanet";
        }

        var exoplanets =
            from response in await _exoplanetApi.GetAllExoplanets()
            where response.Distance.HasValue
            where response.HostName is not null
            where response.Name is not null
            select new SyncronizeExoplanetDto(response.Name!, response.HostName!, response.Distance!.Value, "Parsecs");

        await _db.Set<Exoplanet>().LoadAsync(cancellationToken);

        foreach (var (name, hostName, distance, unit) in exoplanets.Distinct())
        {
            var dbitem = await _db.FindAsync<Exoplanet>(name);

            var imageUrl = dbitem is null
                ? _imageHandler.EnumerateImages().PickRandom().LocalRootPath
                : dbitem.ImageUrl;

            var planet = new Exoplanet(new Distance(unit, distance), hostName, name, imageUrl);

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