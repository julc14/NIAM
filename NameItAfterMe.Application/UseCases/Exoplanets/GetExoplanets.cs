﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MinimalEndpoints;
using NameItAfterMe.Application.Abstractions;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.UseCases.Exoplanets;

[Endpoint(HttpMethods.Get)]
public class GetExoplanets : PagedQuery, IRequest<IEnumerable<ExoplanetDto>>
{
}

public class GetExoplanetHandler : IRequestHandler<GetExoplanets, IEnumerable<ExoplanetDto>>
{
    private readonly IExoplanetContext _db;
    private readonly IMapper _mapper;

    public GetExoplanetHandler(IExoplanetContext db, IMapper mapper)
        => (_db, _mapper) = (db, mapper);

    public async Task<IEnumerable<ExoplanetDto>> Handle(GetExoplanets request, CancellationToken cancellationToken)
    {
        var results = await _db
            .Set<Exoplanet>()
            .OrderBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ExoplanetDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<ExoplanetDto>(request.PageNumber, request.PageSize, results);
    }
}