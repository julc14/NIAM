using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NameItAfterMe.Application.Abstractions;
using Refit;
using Serilog;

namespace NameItAfterMe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // if key is missing use a DEMO key.
        var apiKey = configuration.GetValue("NasaApiKey", "DEMO_KEY");

        var settings = configuration.GetRequiredSection(nameof(ExoplanetSettings)).Get<ExoplanetSettings>();

        return services

            // add endpoint to access NASA picture of the day
            .AddRefitClient<IPictureOfTheDay>()
            .AddHttpMessageHandler(() => new ApiKeyHandler(apiKey))
            .SetHttpBaseAddress("https://api.nasa.gov")

            // add endpoint to acccess exoplanet repository.
            .AddRefitClient<IExoplanetService>()
            .SetHttpBaseAddress("https://exoplanetarchive.ipac.caltech.edu")

            // add respositories for application services.
            // we could bind REFIT created infra services directly to the abstraction living in app layer
            // however this would force application abstractions to depend upon REFIT
            // instead maintain clear boundaries between infra and application services.
            .AddTransient<IPictureOfTheDayRepository, PictureOfTheDayRepository>()
            .AddTransient<IExoplanetApi, ExoplanetRepository>()
            .AddTransient<IImageHandler, ImageHandler>()

            // add cosmos
            .AddDbContext<ExoplanetContext>(options => options.UseCosmos(
                $"AccountEndpoint={settings.AccountEndpoint};AccountKey={settings.AccountKey};",
                databaseName: settings.Name))
            .AddScoped<IExoplanetContext>(sp => sp.GetRequiredService<ExoplanetContext>())

            // add general infra.
            .AddAutoMapper(e => e.AddMaps(typeof(DependencyInjection).Assembly))
            .AddLogging(configure => configure.AddSerilog());
    }

    // trivial extention method to enable a fluent service creation above
    // by ending an http configuration and returning default services.
    private static IServiceCollection SetHttpBaseAddress(
        this IHttpClientBuilder builder,
        string url)
    {
        _ = builder.ConfigureHttpClient(client => client.BaseAddress = new Uri(url));
        return builder.Services;
    }
}
