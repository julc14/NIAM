using MediatR;

namespace NameItAfterMe.Application.UseCases.Exoplanets;

public class PostExoplanet : IRequest
{
    public string? Name { get; set; }
    public int Distance { get; set; }
}

public class PostExoplanetHandler : IRequestHandler<PostExoplanet>
{
    public PostExoplanetHandler()
    {

    }

    public Task<Unit> Handle(PostExoplanet request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Unit.Value);
    }
}