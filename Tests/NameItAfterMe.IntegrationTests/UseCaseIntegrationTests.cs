using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NameItAfterMe.Application.UseCases.Exoplanets;
using NameItAfterMe.Application.UseCases.PictureOfTheDay;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace NameItAfterMe.IntegrationTests;

public class UseCaseIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _host;

    internal IServiceScope CreateScope => _host.Services.CreateScope();

    public UseCaseIntegrationTests(
        WebApplicationFactory<Program> testHost)
            => _host = testHost;

    [Fact]
    public async Task GetPictureOfTheDayStream_ReturnsStreamOfBitsNotEmpty()
    {
        var client = _host.CreateClient();
        var soucePath = await client.GetFromJsonAsync<string>("PictureOfTheDay/SourcePath");

        Assert.NotEmpty(soucePath);
    }

    [Fact]
    public async Task GetExoplanetChunk_ReturnsValidExoplanets()
    {
        using var scope = CreateScope;
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var chunks = await mediator.Send(new GetExoplanets()
        {
            PageNumber = 1,
            PageSize = 5,
        });

        // could return less than 5 if there are less than 5 objects at { StartIndex + ChunkSize }
        // but never more than 5
        Assert.True(chunks.Count() <= 5);
        // always atleast 1 element.
        Assert.NotEmpty(chunks);
    }
}
