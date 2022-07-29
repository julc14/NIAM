using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NameItAfterMe.Application.Behavior;

namespace NameItAfterMe.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
        => services
        .AddMediatR(typeof(DependencyInjection).Assembly)
        .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly)
        .AddAutoMapper(c => c.AddMaps(typeof(DependencyInjection).Assembly))
        .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
        .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
}