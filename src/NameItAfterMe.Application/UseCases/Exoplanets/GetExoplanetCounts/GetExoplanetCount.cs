using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinimalEndpoints;
using NameItAfterMe.Application.Domain;
using NameItAfterMe.Infrastructure.Persistance;
using Newtonsoft.Json;

namespace NameItAfterMe.Application.UseCases.Exoplanets.GetExoplanetCounts;

[Endpoint(Route = "Exoplanet/Count")]
public class GetExoplanetCount : IRequest<ExoplanetCountDto>
{
}

public class GetExoplanetCountHandler : IRequestHandler<GetExoplanetCount, ExoplanetCountDto>
{
    private readonly ExoplanetContext _db;
    private readonly ILogger<GetExoplanetCountHandler> _logger;

    public GetExoplanetCountHandler(ExoplanetContext db, ILogger<GetExoplanetCountHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ExoplanetCountDto> Handle(GetExoplanetCount request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(JsonConvert.SerializeObject(request));

        var exoplanets = _db.Set<Exoplanet>();

        return new ExoplanetCountDto()
        {
            UnnamedExoplanets = await exoplanets.CountAsync(x => x.ProvidedName == null, cancellationToken),
            NamedExoplanets = await exoplanets.CountAsync(x => x.ProvidedName != null, cancellationToken),
        };
    }
}