# Name It After Me

This is a learning excersise targeting 3 goals:

1. Develop a simple website with Blazor:
    - Use Blazor (client side) tooling to build a public UI that delivers the ability to name stars and exoplanets.
    - ASP.Net backend to support accessing NASA APIs (which use cors)
2. Expirement with Minimal APIs:
    - Create functionality that can automaticlly host a mediar use case to an endpoint.
    - Backend will employ Hexagonal Architecture and Mediatr.
    - Should support OpenAPI (swagger)
3. Utilize Azure DevOps+Tooling:

## Blazor WASM Front End:
### Goals:



- [x] Keep application code out of the presentation layer
- [x] Develop Infinite Scrolling component to load unnamed exoplanets 
- [x] Display NASA's picture of the day
- [ ] User can query named exoplanets
- [ ] User can name unnamed exoplanets
- [ ] Develop Some sort of dashboard

### Out-Of-Scope
- Authentication
- Authorization

## ASP.NET Core Back End:

An app of this size/complexity would be fine collapsing the Application and Infrastructure layer into the Server project. However it's more interesting to presume this will be a skeleton for a long-term complex bussiness application. In this case the long-term health of the solution requires a more sustainable architectural design. Therefore we will employ a hexagonal/clean architecture style with a Mediatr-dependent service layer (mostly I just want to play with Mediatr).

Employing medaitr gives rise to a system where application concerns live in the application layer and the controller will only tend to be concerned with validation/authentication/authorization.

Ex:
```cs
[HttpGet]
[Authorize(Roles="Administrators")]
public async Task<IActionResult> Get([FromServices] IMediator mediator, DateOnly day)
{
    //validate day 

    var fileStream = await mediator.Send(new GetPictureOfTheDay());
    return File(fileStream, "image/jpg");
}
```

Since authentication+authorization are out of scope for this project we can ignore those (for now), and place validation logic as part of medaitr pipeline behavior. This will leave most controller methods as bolier-plate redirects.  

### Minimal Endpoints

Let's give the ASP.Net core backend a way to automatically host the mediatr use case behind an endpoint, without requiring a boiler-plate controller method.

With a marker attribute (Endpoint) we can instruct asp.net to automaticlly generate the endpoint at startup. Now no controller is required.
```cs
[Endpoint(Route = "Exoplanet/{PageNumber}/{PageSize}")]
public class GetExoplanets : IRequest<IEnumerable<ExoplanetDto>> 
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}

public class GetExoplanetHandler : IRequestHandler<GetExoplanets, IEnumerable<ExoplanetDto>>
{
    private readonly IExoplanetContext _db;
    private readonly IMapper _mapper;

    public GetExoplanetHandler(IExoplanetContext db, IMapper mapper)
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
```

### Goals

- [x] Mediatr use cases with 'Endpoint' are automaticlly hosted by ASP.Net 
- [x] Mediatr requests are bound from query parameters, route parameters, and sometimes the request body.

## Azure DevOps (Free-Tier)

### Goals
- [x] Employ Azure Cosmos with EF ORM
- [x] Employ Azure App Configuration to host connectionstrings/appconfigurations
- [x] Host the backend through Azure App Service
- [x] Complete a full CI/CD pipeline (ignoring unit tests for now)