using FluentValidation;
using NameItAfterMe.Application.UseCases.Exoplanets.GetExoplanetCounts;
using NameItAfterMe.Application.UseCases.Exoplanets.GetExoplanets;

namespace NameItAfterMe.IntegrationTests;

public class ExoplanetIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _host;

    public ExoplanetIntegrationTests(
        WebApplicationFactory<Program> testHost)
            => _host = testHost;

    [Fact]
    public async Task GetExoplanetCountUseCase_ReturnsNonZeroCounts()
    {
        var counts = await _host.CreateClient().GetFromJsonAsync(new GetExoplanetCount());

        Assert.NotNull(counts);
        Assert.True(counts.TotalExoplanets > 0);
    }

    [Fact]
    public async Task GetExoplanetUseCase_ReturnsRequestedExoplanets()
    {
        var planets = await _host.CreateClient().GetFromJsonAsync(new GetExoplanets()
        {
            PageNumber = 1,
            PageSize = 5
        });

        Assert.NotNull(planets);
        Assert.True(planets.Count() == 5);
    }


    [Fact]
    public async Task GetExoplanetUseCase_ThrowsOnNegateRequests()
    {
        var request = async () => await _host.CreateClient().GetFromJsonAsync(new GetExoplanets()
        {
            PageNumber = -1,
            PageSize = -1
        });

        await Assert.ThrowsAsync<HttpRequestException>(request);
    }
}
