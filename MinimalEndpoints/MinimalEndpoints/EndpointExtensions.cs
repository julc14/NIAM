using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MinimalEndpoints.RequestBinding;
using System.Reflection;
using FluentValidation;

namespace MinimalEndpoints;

public static class EndpointExtensions
{
    /// <summary>
    ///     Automatically host mediatr request handlers from the given assembly that contain an Endpoint marker.
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
            async Task HandleEndpointAction(HttpContext context, IMediator mediatr, RequestBuilder mediatrRequestBuilder)
            {
                // there are better ways to handler asp.net request => mediatr request binding
                // e.g... IParameter<T> DI which would allow customization of binding by request type if desired
                // however dotnet 7 will fix this awkwardness completely with [FromParameters] attribute so leave it as is for now
                // this will also remove the need for custom swagger handling.
                var request = mediatrRequestBuilder.Build(endpoint.RequestType, context);

                var content = await mediatr.Send(request, cancellationToken: context.RequestAborted);

                switch (content)
                {
                    case Stream streamedResponse:
                        context.Response.ContentType =
                            endpoint.ContentType ?? context.Response.ContentType;

                        await streamedResponse.CopyToAsync(context.Response.Body, context.RequestAborted);
                        await streamedResponse.DisposeAsync();

                        return;

                    // Unit type is fake void from mediatr. Only write if not unit.
                    case not Unit:
                        await context.Response.WriteAsJsonAsync(content, context.RequestAborted);
                        return;
                }

                var body = context.Response.Body;
                await body.FlushAsync(cancellationToken: context.RequestAborted);
            }

            builder
            .Map(endpoint.Route, HandleEndpointAction)
            .Produces(200, endpoint.ResponseType, endpoint.ContentType)
            .ProducesValidationProblem(422, "application/json")
            .ConfigureAccepts(endpoint)
            .WithMetadata(new HttpMethodMetadata(new[] { endpoint.HttpMethod.ToString() }))
            .WithMetadata(endpoint);
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
        services.AddScoped<IComponentParser, RequestBodyParser>();
        services.AddScoped<IComponentParser, QueryParametersParser>();
        services.AddScoped<IComponentParser, RouteValuesParser>();
        services.AddScoped<RequestBuilder>();

        return services;
    }

    private static RouteHandlerBuilder ConfigureAccepts(this RouteHandlerBuilder builder, EndpointMetadata endpoint)
    {
        // Get+Trace cannot have a request body.
        if (endpoint.HttpMethod != HttpMethods.Get && endpoint.HttpMethod != HttpMethods.Trace)
        {
            return builder.Accepts(endpoint.RequestType, "application/json");
        }

        return builder;
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

            let httpMethod = endpointAttribute.HttpMethod
                ?? InferHttpMethodFromRequestType(requestType)
                ?? throw new InvalidOperationException($"Cannot select any HttpProtocal for {requestType}")

            select new EndpointMetadata()
            {
                RequestType = requestType,
                ResponseType = responseType,
                ContentType = endpointAttribute.ContentType,
                HttpMethod = httpMethod,
                Route = endpointAttribute.Route ?? requestType.Name
            };
    }

    private static HttpMethods? InferHttpMethodFromRequestType(Type requestType)
    {
        var httpNames = Enum.GetNames<HttpMethods>();
        var httpMethod = Array.Find(httpNames, httpMethod => requestType.Name.StartsWith(httpMethod));

        if (httpMethod is null)
            return null;

        return Enum.Parse<HttpMethods>(httpMethod);
    }
}