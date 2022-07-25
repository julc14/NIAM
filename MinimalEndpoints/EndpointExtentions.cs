using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MinimalEndpoints.RequestBinding;
using System.Reflection;

namespace MinimalEndpoints;

public static class EndpointExtentions
{
    /// <summary>
    ///     Automaticlly host mediatr request handlers from the given assembly that contain an Endpoint marker.
    /// </summary>
    /// <param name="builder">
    ///     The endpoint route builder.
    /// </param>
    /// <param name="assembly">
    ///     The assembly to search.
    /// </param>
    /// <returns>
    ///     The endpoint route builder.
    /// </returns>
    public static IEndpointRouteBuilder MapUseCasesFromAssembly(
        this IEndpointRouteBuilder builder,
        Assembly assembly)
    {
        foreach (var endpoint in ScanForEndpoints(assembly))
        {
            // HttpResponse is not needed as a parameter below and could be omitted.
            // However doing so leads to Swagger failing to generate the endpoint documentation.
            // Hopefully this gets fixed with dotnet7
            // https://github.com/dotnet/aspnetcore/issues/39766
            async Task HandleEndpointAction(HttpContext context, HttpResponse _)
            {
                // it would be better to let ASP.net handle the web request => medaitr request binding
                // This would take advantage of existing and familiar systems (and lead to openAPI working as expected without workarounds)
                // intead we are creating a class (RequestBuilder) to do it manually
                // Using asp.net binding would require implementing a Bind/Parse for each medaitr request type.
                // Forcing each mediatr request type to implement one of these methods is not realy "minimal" so not an option.
                // todo: investigate source generators to create these methods as an alternative to below
                var mediatrRequestBuilder = context.RequestServices.GetRequiredService<RequestBuilder>();
                var request = mediatrRequestBuilder.Build(endpoint.RequestType, context);

                var mediatr = context.RequestServices.GetRequiredService<IMediator>();
                var response = await mediatr.Send(request, cancellationToken: context.RequestAborted);

                await HandleResponse(context, endpoint, response);

                var body = context.Response.Body;
                await body.FlushAsync(cancellationToken: context.RequestAborted);
            }

            builder
            .Map(endpoint.Route, HandleEndpointAction)
            .Produces(200, endpoint.ResponseType, endpoint.ContentType)
            .ProducesProblem(404)
            .WithMetadata(new HttpMethodMetadata(new[] { endpoint.HttpMethod }))
            .WithMetadata(endpoint);

            // todo: httpposts should accept mediatr request body.
        }

        return builder;
    }

    /// <summary>
    ///     Add required services to support Minimal Endpoints.
    /// </summary>
    /// <param name="services">
    ///     The services collection.
    /// </param>
    /// <returns>
    ///     Services.
    /// </returns>
    public static IServiceCollection AddMinimalEndpointServices(this IServiceCollection services)
    {
        services.AddScoped<IComponentParser, ParsePropertyFromQueryParameters>();
        services.AddScoped<IComponentParser, ParsePropertiesFromRouteValues>();
        services.AddScoped<RequestBuilder>();

        return services;
    }

    private static IEnumerable<EndpointMetadata> ScanForEndpoints(
        Assembly assembly)
    {
        var mediatrRequestInterfaceTypes = new Type[]
        {
            typeof(IRequest<>),
            typeof(IStreamRequest<>),
        };

        return
            from requestType in assembly.GetTypes()
            let mediatrInterfaces =
                from i in requestType.GetInterfaces()
                where i.IsGenericType
                let genericTypeDefinition = i.GetGenericTypeDefinition()
                where mediatrRequestInterfaceTypes.Contains(genericTypeDefinition)
                select i

            let attributes = requestType.GetCustomAttributes(inherit: false)
            let endpointAttribute = attributes.OfType<EndpointAttribute>().FirstOrDefault()

            where mediatrInterfaces.Any() && endpointAttribute is not null

            let medaitrInterface = mediatrInterfaces.First()
            let responseType = medaitrInterface.GetGenericArguments().First()

            select new EndpointMetadata(
                requestType,
                responseType,
                endpointAttribute.Route ?? requestType.Name,
                endpointAttribute.HttpMethod)
            {
                ContentType = endpointAttribute.ContentType,
            };
    }

    private static async Task HandleResponse(
        HttpContext context,
        EndpointMetadata endpoint,
        object response)
    {
        switch (response)
        {
            case Stream streamedResponse:
            {
                context.Response.ContentType =
                    endpoint.ContentType ?? context.Response.ContentType;

                await streamedResponse.CopyToAsync(context.Response.Body, context.RequestAborted);
                break;
            }

            case not Unit:
            {
                await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
                break;
            }
        }
    }
}