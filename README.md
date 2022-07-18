# Name It After Me

This is a learning excersise targeting 3 goals:

1. Develop a simple website with Blazor:
    - Use Blazor (client side) tooling to build a public UI that delivers the ability to name stars and exoplanets.
    - ASP.Net backend to support accessing NASA APIs (which use cors)
2. Expirement with Minimal APIs:
    - Create functionality that can automaticlly host a mediar use case to an endpoint.
    - Project will employ Hexagonal Architecture and Mediatr.
    - Should support OpenAPI (swagger)
3. Utilize Azure DevOps+Tooling:
     - Azure Cosmos Db
     - Azure KeyVault/AppConfiguration
     - Azure App Service (Web Hosting)
     - Github will handle the build for now

## Blazor 

Given how small this app is, systems like Mediatr + Hexagonal Architecture are not really worth the cost-benifit. However we will think of this app as a skeleton for a long-term bussiness application where in that context, maintainablity is more important. Hence we will employ them (and other business practices) anyways. For example we will keep Application concerns out of the UI layer and allow the blazor project to focus entirely on the UI.

We will use blazor client-side model as opposed to server-side purely because I've already used server-side before.

UI will use Mudblazor library.

Desired Functionality:


## Minimal Endpoints

Architectures like Hexagonal Architecture will dictate a clear architecural boundary to seperate concerns. This pushes infrastructure concerns out to the boundary of the application and decouples it from application/domain code via dependency injection. As well now we have application code that can't exist within Controllers. And if we employ mediatr this leaves many controllers as simple redirects to the use case app core.

Rather than maintain these boiler-plate redirects we can have ASP.Net automaticlly generate the endpoint and save us from needing the controller method althogether. However this will come at a cost and is only advisable for simple queries/commands. Before the controller could act as a hub for authentication/authorization/validation/pagination. Now these concerns will need be desribed in the application core. If the costs outway the benifits you can still use controllers exactly as you would before.

Example:

Boiler-plate controller method pointing to the application core.
```cs
[HttpGet]
public async Task<IActionResult> Get([FromServices] IMediator mediator)
{
    var fileStream = await mediator.Send(new GetPictureOfTheDay());
    return File(fileStream, "image/jpg");
}
```

Instead with a marker attribute we can instruct asp.net to automaticlly generate the endpoint at startup. Now we can completely remove this method (and possibly the controller altogether!).
```cs
[Endpoint(HttpMethods.Get)]
public class GetExoplanets : PagedQuery, IRequest<IEnumerable<ExoplanetDto>> { }

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

By default the endpoint will generate a flat route with the same name as the request. Configure the optional route in the attribute just like you would in the controller

```cs
[Endpoint(HttpMethods.Get, Route = "/Exoplanet/{PageSize:int}/{PageNumber:int}")]
public class GetExoplanets : PagedQuery, IRequest<IEnumerable<ExoplanetDto>> { }
```