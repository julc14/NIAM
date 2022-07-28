using AutoMapper;
using NameItAfterMe.Application.Abstractions;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Infrastructure;

internal class ExoplanetRepository : IExoplanetApi
{
    private readonly IExoplanetService _exoplanetService;
    private readonly IMapper _mapper;

    public ExoplanetRepository(
        IExoplanetService exoplanetService,
        IMapper mapper)
            => (_exoplanetService, _mapper) = (exoplanetService, mapper);

    /// <summary>
    ///     Get All expolanets.
    /// </summary>
    /// <returns>
    ///     All exoplanets.
    /// </returns>
    public async Task<IEnumerable<Exoplanet>> GetAllExoplanets()
    {
        var validPlanets =
            from p in await _exoplanetService.GetAllPlanets()
            where p.Distance is not null
            where p.Name is not null
            select p;

        return _mapper.Map<IEnumerable<ExoplanetQueryResponse>, IEnumerable<Exoplanet>>(validPlanets);
    }
}
