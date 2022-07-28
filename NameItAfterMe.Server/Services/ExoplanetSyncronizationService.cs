using Microsoft.EntityFrameworkCore;
using NameItAfterMe.Application.Abstractions;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Server.Services;

public class ExoplanetSyncronizationService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ExoplanetSyncronizationService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var exoplanetWebApi = scope.ServiceProvider.GetRequiredService<IExoplanetApi>();
                var db = scope.ServiceProvider.GetRequiredService<IExoplanetContext>();

                var sourceExoplanets = await exoplanetWebApi.GetAllExoplanets();
                var exoplanets = db.Set<Exoplanet>();

                var newPlanets = sourceExoplanets.Except(
                    await exoplanets.ToListAsync(), 
                    new ExoplanetComparer());

                await exoplanets.AddRangeAsync(newPlanets.ToList(), stoppingToken);
                await db.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}
