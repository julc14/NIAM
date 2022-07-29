using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MinimalEndpoints.RequestBinding;
using System.Reflection;
using FluentValidation;

namespace MinimalEndpoints;

public static class EndpointExtentions
{
    private const int ValidationErrorCode = 422;

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
            // it would be better to let ASP.net handle the web request => medaitr request binding
            // This would take advantage of existing and familiar systems (and lead to openAPI working as expected without workarounds)
            // intead we are creating a class (RequestBuilder) to do it manually
            // Using asp.net binding would require implementing a Bind/Parse for each medaitr request type.
            // Forcing each mediatr request type to implement one of these methods is not realy "minimal" so not an option.
            // todo: investigate source generators to create these methods as an alternative to below
            async Task HandleEndpointAction(HttpContext context, IMediator mediatr, RequestBuilder mediatrRequestBuilder)
            {
                var request = mediatrRequestBuilder.Build(endpoint.RequestType, context);
                await ExecuteMediatrUseCase(context, mediatr, request, endpoint.ContentType);

                var body = context.Response.Body;
                await body.FlushAsync(cancellationToken: context.RequestAborted);
            }

            builder
            .Map(endpoint.Route, HandleEndpointAction)
            .Produces(200, endpoint.ResponseType, endpoint.ContentType)
            .ProducesValidationProblem(ValidationErrorCode, "application/json")
            .WithMetadata(new HttpMethodMetadata(new[] { endpoint.HttpMethod.ToString() }))
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
        services.AddScoped<IComponentParser, RequestBodyParser>();
        services.AddScoped<IComponentParser, QueryParametersParser>();
        services.AddScoped<IComponentParser, RouteValuesParser>();
        services.AddScoped<RequestBuilder>();

        return services;
    }

    private static async Task ExecuteMediatrUseCase(HttpContext context, IMediator mediatr, object request, string? contentType = null)
    {
        try
        {
            var content = await mediatr.Send(request, cancellationToken: context.RequestAborted);

            switch (content)
            {
                case Stream streamedResponse:
                    context.Response.ContentType =
                        contentType ?? context.Response.ContentType;

                    await streamedResponse.CopyToAsync(context.Response.Body, context.RequestAborted);
                    await streamedResponse.DisposeAsync();

                    return;

                case not Unit:
                    await context.Response.WriteAsJsonAsync(content, context.RequestAborted);
                    return;
            }
        }
        catch (ValidationException e)
        {
            context.Response.StatusCode = ValidationErrorCode;
            await context.Response.WriteAsJsonAsync(e.Errors);
        }
        catch (Exception)
        {
            context.Response.StatusCode = 500;
        }
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
}