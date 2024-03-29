﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using NameItAfterMe.Application.Abstractions;
using NameItAfterMe.Application.Domain;
using NameItAfterMe.Application.Infrastructure.Nasa.Exoplanet;
using NameItAfterMe.Application.Infrastructure.Persistence;

namespace NameItAfterMe.Application.UseCases.Exoplanets.SynchronizeExoplanets;

public class SynchronizeExoplanetData : IRequest
{
}

public class SynchronizeExoplanetDataHandler : IRequestHandler<SynchronizeExoplanetData>
{
    private readonly IExoplanetService _exoplanetApi;
    private readonly ExoplanetContext _db;
    private readonly IImageHandler<ExoplanetImage> _exoplanetImageHandler;

    public SynchronizeExoplanetDataHandler(
        IImageHandler<ExoplanetImage> exoplanetImageHandler,
        IExoplanetService exoplanetApi,
        ExoplanetContext db)
            => (_exoplanetApi, _db, _exoplanetImageHandler) = (exoplanetApi, db, exoplanetImageHandler);

    public async Task<Unit> Handle(SynchronizeExoplanetData request, CancellationToken cancellationToken)
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
            var dbItem = await _db.FindAsync<Exoplanet>(name);

            var planet = new Exoplanet()
            {
                Name = name,
                HostName = hostName,
                Distance = distance,
                ProvidedName = dbItem?.ProvidedName,
                Story = dbItem?.Story,
                // since we are randomly assigning each planet an image, only assign if null.
                // otherwise we will update each item to pointlessly assign a different random image
                ImageUrl = dbItem?.ImageUrl ?? exoplanetImages.PickRandom().Url
            };

            if (dbItem is null)
            {
                _db.Add(planet);
            }
            else
            {
                _db.Entry(dbItem).CurrentValues.SetValues(planet);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}