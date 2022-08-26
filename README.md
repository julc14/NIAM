# Name It After Me

This solution is a learning excerise where the primary goal is to build an end-to-end application with a non-trivial backend and cloud based hosting/tooling. 

The application intent will be to load+store Exoplanet data from Nasa public APIs and give users the ability to name exoplanets after themselves (with an interesting backstory :)). Exoplanets are usually given very boring names (Kepler-55b, p3x500-b)

## Vertial Slice Architecture:

Given the complexity of the application (not very) it should be pointed out this solution does not require a defined backend architecture (and could even be considered counter-productive). We could jam all application logic into an ASP.Net server backend and be fine. However, this would cut the learning experience short. Instead we will presume this application will be a starting point for a long-term bussiness application. Vertical Slice Architecure is my preferred choice. 

![](/Docs/Images/VerticalSlice.png)

Vertical Slice architecture style is about organizing code by features and vertical slices instead of organizing by technical concerns. We will model each feature as a transaction script (mediatr use case) and maximize coupling within the use case.

However, we will still hold on to some layering for infastructure services

### Other Notes:
- Blazar WASM over Blazor Server: To experiment with client-side spa frameworks.
- ASP.NET Server: Required for WASM... Cant access NasaAPIs directly due to CORs restriction on Nasa's side.
- Will use Azure app service hosting and Azure Cosmos + EFCore


### Summary:

Putting all the pieces together here is a general system diagram.

![](/Docs/Images/SolutionDesign.png)

## Checklist:

#### Vertical Slice Architecture:
- [x] Keep application code out of the presentation layer(s)
- [x] Define use cases with Mediatr

#### Blazor UI:
- [x] Build Infinite Scrolling shared component to load unnamed exoplanets 
- [x] Page to display NASA's picture of the day
- [x] Cool Dashboard page
- [ ] User can name unnamed exoplanet and planet can never be named again (Persist to DB).
- [ ] User can provide story as part of the naming process.
- [ ] User can query named exoplanets.

#### ASP.Net Backend:
- [x] Mediatr use cases with 'Endpoint' are automaticlly hosted by ASP.Net 
- [x] Mediatr requests are bound from query parameters, route parameters, and sometimes the request body.
- [x] Automaticlly generated endpoints details are compatible with OpenAPI (swagger)

#### Azure:
- [x] Azure Cosmos with EF ORM
- [x] Azure App Configuration to host connectionstrings/app configurations
- [x] Host the backend through Azure App Service
- [x] Complete a full CI/CD pipeline (ignoring unit tests for now)

#### Tests:
- [ ] Unit Tests
- [ ] Integration Tests

# Minimal Endpoints

(MinimalEndpoints could be its own package but is left here for simplicity)

Controllers are dinosaurs. Rather than hosting the mediatr use case behind controllers lets us MinimalAPIs introduced in .NET6 and host it behind an automatically generated endpoint. Generating the endpoint like this has its down-sides and will restrict usage of some neat functionality MVC will give you, but its not needed for this project anyways.

Here is an example of a mediatr use case:

```cs
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

And a controller method that hosts the use case behind an endpoint may look like:

```cs
[HttpGet]
public async Task<<ActionResult<IEnumerable<ExoplnaetDto>>>> GetPlanets([FromServices] IMediator mediator, int page, int pageSize)
{
    if (page <= 0 || pageSize <= 0)
        return new BadRequest();

    var planets = await mediator.Send(new GetExoplanets() { PageNumber = page, PageSize = pageSize} );
    return Ok(planets);
}
```

As you can see the controller method is performing simple validation and forwarding the requet to mediatr. Many controller method will look similar and boiler-platey.

If we handle validation with mediatr directly the only thing that is left is forwarding to mediatr. We can introduce an attribute and at startup automaticlly create an endpoint for each use case marked with this attribute.

```cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseEndpoints(builder =>
{
    builder.MapUseCasesFromAssembly(typeof(ApplicationService).Assembly);
});

app.Run();
```

Use case now with *Endpoint* attribute:

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

// Not referenced directly, part of pipeline behavior.
public class GetExoplanetValidator : AbstractValidator<GetExoplanets>
{
    public GetExoplanetValidator()
    {
        RuleFor(x => x.PageSize).GreaterThan(0);
        RuleFor(x => x.PageNumber).GreaterThan(0);
    }
}
```

Endpoint attribute can specify the Http method (HttpGet, post.. etc) or if absent can be inferred from the request type. **Get**Exoplanets will be map to an HttpGet.


Now the endpoint will be automatically generated at startup and the controller can be deleted! The mediatr request properties will be bound from the following sources in order of priority
1. Request Body
2. Route Values
3. Query Parameters