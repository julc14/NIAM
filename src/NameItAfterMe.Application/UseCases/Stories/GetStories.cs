using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MinimalEndpoints;
using NameItAfterMe.Application.Domain;
using NameItAfterMe.Infrastructure.Persistance;

namespace NameItAfterMe.Application.UseCases.Stories;

[Endpoint(Route = "Story")]
public class GetStories : IRequest<IEnumerable<StoryDto>>
{
    public string Name { get; set; } = string.Empty;
}

public class GetStoriesHandler : IRequestHandler<GetStories, IEnumerable<StoryDto>>
{
    private readonly ExoplanetContext _db;
    private readonly IMapper _mapper;

    public GetStoriesHandler(ExoplanetContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StoryDto>> Handle(GetStories request, CancellationToken cancellationToken)
    {
        var stories = await _db.Set<Story>()
            .AsNoTracking()
            .Where(x => string.IsNullOrEmpty(request.Name) || x.Name.Equals(request.Name))
            .ProjectTo<StoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return stories;
    }
}