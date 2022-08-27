using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using MinimalEndpoints;
using NameItAfterMe.Application.Domain;
using NameItAfterMe.Application.Infrastructure.Persistence;

namespace NameItAfterMe.Application.UseCases.Exoplanets.GetUnnamedExoplanets;

[Endpoint(Route = "Exoplanet/{PageNumber:int}/{PageSize:int}")]
public class GetUnnamedExoplanets : IRequest<IEnumerable<ExoplanetDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 15;
}

public class GetExoplanetHandler : IRequestHandler<GetUnnamedExoplanets, IEnumerable<ExoplanetDto>>
{
    private readonly ExoplanetContext _db;
    private readonly IMapper _mapper;

    public GetExoplanetHandler(ExoplanetContext db, IMapper mapper)
        => (_db, _mapper) = (db, mapper);

    public async Task<IEnumerable<ExoplanetDto>> Handle(GetUnnamedExoplanets request, CancellationToken cancellationToken)
    {
        return await _db
            .Set<Exoplanet>()
            .OrderBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Where(x => x.ProvidedName == null)
            .ProjectTo<ExoplanetDto>(_mapper.ConfigurationProvider)
            .ToPaginatedResult(request.PageNumber, request.PageSize, cancellationToken);
    }
}