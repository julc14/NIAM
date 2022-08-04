using MediatR;
using MinimalEndpoints;
using NameItAfterMe.Application.Infrastructure.Files;

namespace NameItAfterMe.Application.UseCases.Exoplanets;

[Endpoint(Route = "Exoplanet/ImagePath")]
public class GetExoplanetImageSourcePath : IRequest<string>
{
    public int? Index { get; set; }
}

public class GetExoplanetImageSourcePathHandler : IRequestHandler<GetExoplanetImageSourcePath, string>
{
    private readonly IImageHandler _imageHandler;

    public GetExoplanetImageSourcePathHandler(IImageHandler imageHandler)
    {
        _imageHandler = imageHandler;
        _imageHandler.LocalFolder = Path.Join("Images", "Exoplanet");
    }

    public Task<string> Handle(GetExoplanetImageSourcePath request, CancellationToken cancellationToken)
    {
        if (_imageHandler.TrySearch($"Exoplanet{Random.Shared.Next(1, 10)}", out var image))
        {
            return Task.FromResult(image.LocalRootPath);
        }

        return Task.FromResult("Images\\Exoplanet\\Exoplanet1.jpg");
    }
}
