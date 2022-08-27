using MediatR;
using Microsoft.Extensions.Options;
using NameItAfterMe.Application.UseCases.Exoplanets.SynchronizeExoplanets;

namespace NameItAfterMe.Server.Services;

public class ExoplanetSynchronizationService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ExoplanetSynchronizationService(IServiceScopeFactory serviceScopeFactory)
        => _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<BackgroundServiceOptions>>();

                if (options.Value.IsEnabled)
                {
                    var medaitr = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await medaitr.Send(new SynchronizeExoplanetData(), stoppingToken);
                }
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}