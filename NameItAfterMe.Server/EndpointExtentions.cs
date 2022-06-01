using NameItAfterMe.Application;
using System.Reflection;

namespace NameItAfterMe.Server;

public static class EndpointExtentions
{
    public static IEndpointRouteBuilder MapUseCasesFromAssembly(
        this IEndpointRouteBuilder builder,
        Assembly assembly)
    {
        var useCasesToHost = assembly
            .GetTypes()
            .Select(x => (request: x, metadata: x.GetCustomAttribute<WebHostedUseCaseAttribute>()))
            .Where(x => x.metadata is not null);

        foreach (var (requestType, metadata) in useCasesToHost)
        {
            builder.Map(
                metadata!.Route ?? requestType.Name,
                async builder => await EndpointGeneration.MapEndpoint(requestType, builder, metadata));
        }

        return builder;
    }
}
