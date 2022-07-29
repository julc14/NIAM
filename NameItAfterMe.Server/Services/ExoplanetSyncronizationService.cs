using MediatR;
using NameItAfterMe.Application.UseCases.Exoplanets;

namespace NameItAfterMe.Server.Services;

public class ExoplanetSyncronizationService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ExoplanetSyncronizationService(IServiceScopeFactory serviceScopeFactory)
        => _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var medaitr = scope.ServiceProvider.GetRequiredService<IMediator>();
                await medaitr.Send(new SyncronizeExoplanetData(), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}