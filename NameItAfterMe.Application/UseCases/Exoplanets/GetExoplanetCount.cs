﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using MinimalEndpoints;
using NameItAfterMe.Application.Abstractions;
using NameItAfterMe.Application.Domain;
using NameItAfterMe.Application.Services;

namespace NameItAfterMe.Application.UseCases.Exoplanets;

[Endpoint(Route = "Exoplanet/Count")]
public class GetExoplanetCount : IRequest<int>
{
    public bool ExcludeUnnamedExoplanets { get; set; }
    public bool ExcludeNamedExoplanets { get; set; }
}

public class GetExoplanetCountHandler : IRequestHandler<GetExoplanetCount, int>
{
    private readonly ExoplanetContext _db;

    public GetExoplanetCountHandler(ExoplanetContext db) => _db = db;

    public async Task<int> Handle(GetExoplanetCount request, CancellationToken cancellationToken)
    {
        var exoplanets = _db.Set<Exoplanet>();

        if (request.ExcludeUnnamedExoplanets)
        {
            return await exoplanets.CountAsync(x => x.ProvidedName != null, cancellationToken);
        }

        if (request.ExcludeNamedExoplanets)
        {
            return await exoplanets.CountAsync(x => x.ProvidedName == null, cancellationToken);
        }

        return await exoplanets.CountAsync(cancellationToken);
    }
}