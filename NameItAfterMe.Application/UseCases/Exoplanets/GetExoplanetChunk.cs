using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NameItAfterMe.Application.Abstractions;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.UseCases.Exoplanets;

[WebHostedUseCase(Route = "Exoplanet")]
public class GetExoplanetChunk : IRequest<IEnumerable<ExoplanetDto>>
{
    public int ChunkSize { get; set; } = 15;
    public int StartIndex { get; set; }
}

public class GetExoplanetChunkHandler : IRequestHandler<GetExoplanetChunk, IEnumerable<ExoplanetDto>>
{
    private readonly IExoplanetContext _db;
    private readonly IMapper _mapper;

    public GetExoplanetChunkHandler(IExoplanetContext db, IMapper mapper)
        => (_db, _mapper) = (db, mapper);

    public async Task<IEnumerable<ExoplanetDto>> Handle(GetExoplanetChunk request, CancellationToken cancellationToken)
    {
        return await _db
            .Set<Exoplanet>()
            .OrderBy(x => x.Id)
            .Skip(request.StartIndex)
            .Take(request.ChunkSize)
            .ProjectTo<ExoplanetDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
