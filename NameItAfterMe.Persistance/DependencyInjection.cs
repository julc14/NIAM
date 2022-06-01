using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NameItAfterMe.Application.Abstractions;

namespace NameItAfterMe.Persistance;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistance(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetRequiredSection(nameof(ExoplanetSettings)).Get<ExoplanetSettings>();

        return services
            .AddDbContext<ExoplanetContext>(options => options.UseCosmos(
                $"AccountEndpoint={settings.AccountEndpoint};AccountKey={settings.AccountKey};",
                databaseName: settings.Name))
            .AddScoped<IExoplanetContext>(sp => sp.GetRequiredService<ExoplanetContext>());
    }
}
