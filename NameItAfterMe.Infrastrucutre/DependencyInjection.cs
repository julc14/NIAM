using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NameItAfterMe.Application.Abstractions;
using Refit;

namespace NameItAfterMe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // if key is missing use a DEMO key.
        var apiKey = configuration.GetValue("NasaApiKey", "DEMO_KEY");

        var settings = configuration.GetRequiredSection(nameof(ExoplanetSettings)).Get<ExoplanetSettings>();

        services
        .AddRefitClient<IPictureOfTheDay>()
        .AddHttpMessageHandler(() => new ApiKeyHandler(apiKey))
        .SetHttpBaseAddress("https://api.nasa.gov")

        .AddRefitClient<IExoplanetService>()
        .SetHttpBaseAddress("https://exoplanetarchive.ipac.caltech.edu")

        .AddTransient<IPictureOfTheDayRepository, PictureOfTheDayRepository>()
        .AddTransient<IExoplanetApi, ExoplanetRepository>()
        .AddTransient<IImageHandler, ImageHandler>()

        .AddDbContext<ExoplanetContext>(options => options.UseCosmos(
            $"AccountEndpoint={settings.AccountEndpoint};AccountKey={settings.AccountKey};",
            databaseName: settings.Name))
        .AddScoped<IExoplanetContext>(sp => sp.GetRequiredService<ExoplanetContext>())

        .AddAutoMapper(e => e.AddMaps(typeof(DependencyInjection).Assembly));

        return services;
    }

    private static IServiceCollection SetHttpBaseAddress(
        this IHttpClientBuilder builder,
        string url)
    {
        _ = builder.ConfigureHttpClient(client => client.BaseAddress = new Uri(url));
        return builder.Services;
    }
}
