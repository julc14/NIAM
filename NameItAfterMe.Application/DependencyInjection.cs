using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NameItAfterMe.Application.Behavior;
using NameItAfterMe.Application.UseCases.Exoplanets;

namespace NameItAfterMe.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
            .AddMediatR(typeof(DependencyInjection).Assembly)
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly)
            // todo: auto register base class validators.
            .AddTransient<IValidator<GetExoplanets>, PagedQueryValidator>()
            .AddAutoMapper(c => c.AddMaps(typeof(DependencyInjection).Assembly))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
}