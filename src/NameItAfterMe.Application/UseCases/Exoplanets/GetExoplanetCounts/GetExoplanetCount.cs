using MediatR;
using Microsoft.EntityFrameworkCore;
using MinimalEndpoints;
using NameItAfterMe.Application.Domain;
using NameItAfterMe.Infrastructure.Persistance;

namespace NameItAfterMe.Application.UseCases.Exoplanets.GetExoplanetCounts;

[Endpoint(Route = "Exoplanet/Count")]
public class GetExoplanetCount : IRequest<ExoplanetCountDto>
{
}

public class GetExoplanetCountHandler : IRequestHandler<GetExoplanetCount, ExoplanetCountDto>
{
    private readonly ExoplanetContext _db;

    public GetExoplanetCountHandler(ExoplanetContext db) => _db = db;

    public async Task<ExoplanetCountDto> Handle(GetExoplanetCount request, CancellationToken cancellationToken)
    {
        var exoplanets = _db.Set<Exoplanet>();

        return new ExoplanetCountDto()
        {
            UnnamedExoplanets = await exoplanets.CountAsync(x => x.ProvidedName == null, cancellationToken),
            NamedExoplanets = await exoplanets.CountAsync(x => x.ProvidedName != null, cancellationToken),
        };
    }
}