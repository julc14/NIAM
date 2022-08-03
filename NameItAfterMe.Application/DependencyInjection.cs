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
using NameItAfterMe.Application.UseCases.PictureOfTheDay;
using NameItAfterMe.Infrastructure.Persistance;
using Refit;

namespace NameItAfterMe.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMediatR(typeof(DependencyInjection).Assembly)
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly)
            .AddAutoMapper(c => c.AddMaps(typeof(DependencyInjection).Assembly))
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
            .AddTransient<PictureOfTheDayImageHandler>()

            .AddDbContext<ExoplanetContext>(options =>
            {
                options.UseCosmos(
                    $"AccountEndpoint={settings.AccountEndpoint};AccountKey={settings.AccountKey};",
                    databaseName: settings.Name);
            });

        services.AddScoped<IRequestHandler<GetPictureOfTheDaySourcePath, string>>(sp =>
            ActivatorUtilities.CreateInstance<GetPictureOfTheDaySourcePathHandler>(
                sp,
                sp.GetRequiredService<PictureOfTheDayImageHandler>())
        );

        return services;
    }
    GetPictureOfTheDaySourcePathHandler

    private static IServiceCollection SetHttpBaseAddress(
        this IHttpClientBuilder builder,
        string url)
    {
        _ = builder.ConfigureHttpClient(client => client.BaseAddress = new Uri(url));
        return builder.Services;
    }
}