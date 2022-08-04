using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NameItAfterMe.Application.Behavior;
using NameItAfterMe.Application.Infrastructure.Files;
using NameItAfterMe.Application.Infrastructure.Nasa.Exoplanet;
using NameItAfterMe.Application.Infrastructure.Nasa.PictureOfTheDay;
using NameItAfterMe.Application.Infrastructure.PictureOfTheDay;
using NameItAfterMe.Infrastructure.Persistance;
using Refit;

namespace NameItAfterMe.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services
            .AddMediatR(assembly)
            .AddValidatorsFromAssembly(assembly)
            .AddAutoMapper(c => c.AddMaps(assembly))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // if key is missing use a DEMO key value.
        var apiKey = configuration.GetValue("NasaApiKey", "DEMO_KEY");
        var settings = configuration.GetRequiredSection(nameof(ExoplanetSettings)).Get<ExoplanetSettings>();

        services
            .AddRefitClient<IPictureOfTheDayService>()
            .AddHttpMessageHandler(() => new ApiKeyHandler(apiKey))
            .SetHttpBaseAddress("https://api.nasa.gov")

            .AddRefitClient<IExoplanetService>()
            .SetHttpBaseAddress("https://exoplanetarchive.ipac.caltech.edu")

            .AddTransient<IImageHandler, ImageHandler>()

            .AddDbContext<ExoplanetContext>(options =>
            {
                options.UseCosmos(
                    $"AccountEndpoint={settings.AccountEndpoint};AccountKey={settings.AccountKey};",
                    databaseName: settings.Name);
            });

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