using MediatR;
using MinimalEndpoints;
using NameItAfterMe.Application.Domain;
using NameItAfterMe.Application.Infrastructure.Persistence;

namespace NameItAfterMe.Application.UseCases.Exoplanets.NameExoplanet;

[Endpoint(HttpMethods.Post, Route = "Exoplanet/Name")]
public class NameExoplanet : IRequest
{
    public required string Name { get; set; }
    public required string GivenName { get; set; }
    public required string StoryName { get; set; }
    public IEnumerable<string> SelectedWords { get; set; } = Enumerable.Empty<string>();
}

public class NameExoplanetHandler : IRequestHandler<NameExoplanet>
{
    private readonly ExoplanetContext _db;

    public NameExoplanetHandler(ExoplanetContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(NameExoplanet request, CancellationToken cancellationToken)
    {
        var planet = await _db
            .Set<Exoplanet>()
            .FindAsync(request.Name);

        if (planet is null)
        {
            throw new InvalidOperationException($"Invalid Name, {request.Name} is not found in db");
        }

        string? storyBody = null;

        if (!string.IsNullOrEmpty(request.StoryName))
        {
            var story = await _db.Set<Story>().FindAsync(request.StoryName);

            if (story is null)
            {
                throw new InvalidOperationException("Invalid story Id");
            }

            storyBody = story.Format(request.SelectedWords);
        }

        planet.NameIt(request.GivenName, storyBody);

        await _db.SaveChangesAsync();
        return Unit.Value;
    }
}