# Minimal Endpoints

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