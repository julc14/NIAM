# Name It After Me

This is a learning excersise targeting 3 goals:
1. Use .Net 6 Minimal API with mediatr to automatically host the mediatr-defined application use cases at a generated endpoint. This can be nice for Hexagonal/Clean architectures that emphanize application code never living in the presentation layers. In these scenarios controllers often become boiler-plate redirects to the request handler via mediatr. With Net 6 Minimal Apis we can automaticlly generate the endpoint and avoid this.
2. Learn and Improve Blazor skills
3. Learn and Improve Azure Devops skillset
5. Eventually - rebuild the website using typescript + vue.


## Minimal APIs and Mediatr

Architectures like Hexagonal Architecture of Clean Architecture will empahize a clear architecural boundary to seperate concerns. This often pushes infrastructure concerns out to the boundary of the application and isolates application code within the application core.

This seperation often leaves Controllers as a boiler-plate husk that just directs flow into the application use case. Especially if we employ mediatr and FluentValidation to attach validation as a pipeline behavior (as this solution does).

Solutions that employ mediatr and this architecural design pattern can use minimal APIs to autogenerate these controller methods. This will come at a cost (main one being we cant decorate endpoint with some IActionResult) and may not be advisable for more complicated endpoints, however for simple queries we can use this pattern and not lose any valuable functionality while reducing boiler-plate waste and maintaining the seperation of concerns between layers.


Example:

Boiler-plate controller methods pointing to the real actor found in application core. This is preferred to leaving logic in the controller as it decouples the use case from the execution environment and makes it easier to replace infrastructure services (These maintainability benifts are not really needed for this simple project but can be very benificial for larger, more complex applications). 
```cs
[HttpGet]
public async Task<IActionResult> Get([FromServices] IMediator mediator)
{
    var fileStream = await mediator.Send(new GetPictureOfTheDayStream());
    return File(fileStream, "image/jpg");
}
```

Instead with a marker attribute we can instruct our server to automaticlly generate the endpoint at startup and completely remove this method.
```cs
[GenerateEndpoint(ContentType = "image/jpg", Route = "GetPictureOfTheDay/{isHd}")]
public class GetPictureOfTheDayStream : IRequest<Stream>
{
    public bool IsHd { get; set; } = true;
}

public class GetPictureOfTheDayStreamHandler : IRequestHandler<GetPictureOfTheDayStream, Stream>
{
    private readonly HttpClient _httpClient;
    private readonly IPictureOfTheDayRepository _pictureOfTheDayRepository;

    public GetPictureOfTheDayStreamHandler(
        HttpClient httpClient,
        IPictureOfTheDayRepository pictureOfTheDayRepository)
            => (_httpClient, _pictureOfTheDayRepository)
            = (httpClient, pictureOfTheDayRepository);

    public async Task<Stream> Handle(GetPictureOfTheDayStream request, CancellationToken cancellationToken)
    {
        var picOfDay = await
            _pictureOfTheDayRepository.GetPictureOfTheDay().ConfigureAwait(false);

        var imageGet = await _httpClient.GetAsync(picOfDay.Url, cancellationToken).ConfigureAwait(false);
        var mediaType = imageGet.Content.Headers?.ContentType?.MediaType;

        if (mediaType?.Contains("image", StringComparison.InvariantCultureIgnoreCase) == true)
        {
            return await _httpClient.GetStreamAsync(picOfDay.Url, cancellationToken).ConfigureAwait(false);
        }

        return Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("NameItAfterMe.Application.UseCases.PictureOfTheDay.BaseBackground.jpg")
          ?? throw new InvalidOperationException();
    }
}
```

Once we generate the endpoint we still need to find a way to bind the incoming request to the mediatr request object. For now this solution will provide configuration options to customize from where the requset is parsed. In the future this may be customized to provide granular control of each endpoint.

```cs
app.UseEndpoints(builder =>
{
    builder.MapUseCasesFromAssembly(typeof(GenerateEndpointAttribute).Assembly,
    options =>
    {
        // order and presence of these options matter
        // for example query parameters will take precednce over same-name parameters found in the request body.
        options.ParseRequestPropertiesFromBody();
        options.ParseRequestPropertiesFromRouteData();
        options.ParseRequestPropertiesFromQueryParameters();
    });
});
```

By default the endpoint will generate a flat route with the same name as the request. Configure the optional route in the attribute just like you would in the controller

```cs
[GenerateEndpoint(ContentType = "image/jpg", Route = "NASA/GetPictureOfTheDay/{isHd}")]
public class GetPictureOfTheDayStream : IRequest<Stream>
{
    public bool IsHd { get; set; } = true;
}
```

Not supported (yet)
- HttpRedirects