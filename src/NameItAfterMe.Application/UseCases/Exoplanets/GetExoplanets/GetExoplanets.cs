using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using MinimalEndpoints;
using NameItAfterMe.Application.Domain;
using NameItAfterMe.Infrastructure.Persistance;

namespace NameItAfterMe.Application.UseCases.Exoplanets.GetExoplanets;

[Endpoint(Route = "Exoplanet/{PageNumber:int}/{PageSize:int}")]
public class GetExoplanets : IRequest<IEnumerable<ExoplanetDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}

public class GetExoplanetHandler : IRequestHandler<GetExoplanets, IEnumerable<ExoplanetDto>>
{
    private readonly ExoplanetContext _db;
    private readonly IMapper _mapper;

    public GetExoplanetHandler(ExoplanetContext db, IMapper mapper)
        => (_db, _mapper) = (db, mapper);

    public async Task<IEnumerable<ExoplanetDto>> Handle(GetExoplanets request, CancellationToken cancellationToken)
    {
        return await _db
            .Set<Exoplanet>()
            .OrderBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ExoplanetDto>(_mapper.ConfigurationProvider)
            .ToPaginatedResult(request.PageNumber, request.PageSize, cancellationToken);
    }
}