# Name It After Me

This solution is primarily a learning excerise to experience with the primary goal of building an end-to-end application requiring a non-trivial backend. As well employing tooling that is where I lack a depth of experience. The application intent will be to load+store Exoplanet data from Nasa public APIs and give users the ability to name exoplanets after themselves with an interesting backstory. Exoplanets are usually given very boring names (Kepler-55b, p3x500-b)

## Architecture and Tooling Decisions:

### Vertial Slice Architecture:

It should be pointed out a solution of this size/complexity does not require a defined architecture (and could even be considered counter-productive). We could pack everything into an ASP.Net server project and jam logic in controllers and be fine here. However, this would cut the learning experience short. Instead we will presume this application will be a starting point for a long-term bussiness application. Vertical Slice Architecure is my preferred choice for scenarios like this. 

![](/Docs/Images/Verticalslice.png)

Vertical Slice architecture style is about organizing code by features and vertical slices instead of organizing by technical concerns. It's about an idea of grouping code according to the business functionality and putting all the relevant code close together. Vertical Slice architecture can be a starting point and can be evolved later when an application become more sophisticated.

### Blazor Web Assembly (WASM):

We will use Blazor WASM as opposed to Server for the UI. Some day I may rewrite the UI with typescript and WASM will be more analagous than a Blazor Server implementation.

Blazor WASM will require an ASP.NET backend to support functionality. HttpClient cannot access Nasa APIs directly due to CORS restrictions.

### ASP.NET Backend:

I'm not a fan of controllers and want to expirement with alternatives. We will exclusivley use Minimal APIs. In addition I want to develop a package that can automaticlly host mediatr use cases behind an endpoint.

### DB + Azure Hosting:

Cloud-based hosting is a must in 2022. Lets use (free) Azure-Cosmos for storage and (free) Azure app service for hosting.

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

# Minimal Endpoints

(Minimal Endpoint should be its own solution but is left here for simplicity)

Employing medaitr with Vertical Slice Architecture gives rise to a system where application concerns live in the application layer and the controller responsibilities are minimized.

Here is an example use case living in the application layer:

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

In this case we could completly eliminate the controller method by:
- Infering the Http method type from the request name.
- Validation done in application layer (FluentValidation)
- Automatically bind request parameters/values to the mediatr request object (and send the request to mediatr).
- Attribute to let asp.net know which use cases to host.

Now the file may look like this. And we can completly remove the controller.

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

public class GetExoplanetValidator : AbstractValidator<GetExoplanets>
{
    public GetExoplanetValidator()
    {
        RuleFor(x => x.PageSize).GreaterThan(0);
        RuleFor(x => x.PageNumber).GreaterThan(0);
    }
}
```