using MediatR;
using NameItAfterMe.Application;
using System.Reflection;

namespace NameItAfterMe.Server;

public static class EndpointExtentions
{
    /// <summary>
    ///     Maps mediatr use cases that contain GenerateEndpoint marker attribute to an endpoint
    /// </summary>
    /// <param name="builder">
    ///     The endpoint builder.
    /// </param>
    /// <param name="assembly">
    ///     The assembly to search.
    /// </param>
    /// <param name="configureRequestDelegate">
    ///     Optional. Configure how the request is parsed
    ///     Default: parse request from body, then query params, then route data.
    /// </param>
    /// <returns>
    ///     The builder.
    /// </returns>
    public static IEndpointRouteBuilder MapUseCasesFromAssembly(
        this IEndpointRouteBuilder builder,
        Assembly assembly,
        Action<IConfigureRequestBuilder>? configureRequestDelegate = null)
    {
        var mediatrRequestTypes = new List<Type>()
        {
            typeof(IRequest<>),
            typeof(IStreamRequest<>),
        };

        var endpointsToGenerate = assembly
            .GetTypes()
            .Where(x =>
                x.GetInterfaces().Any(x =>
                    x.IsGenericType && mediatrRequestTypes.Contains(x.GetGenericTypeDefinition())))
            .Select(x => (request: x, metadata: x.GetCustomAttribute<GenerateEndpointAttribute>()))
            .Where(x => x.metadata is not null);

        foreach (var (requestType, metadata) in endpointsToGenerate)
        {
            // we could verify the request object has a handler here. If it does not we will get runtime exceptions.
            // However mediatr will do this for us.
            builder.Map(
                metadata!.Route ?? requestType.Name,
                async context =>
                {
                    // ensure the body can be read multiple times
                    // this may be unnessary for this project but can't be sure all middleware will not attempt to 
                    // read the request body.
                    // todo: revisit this
                    context.Request.EnableBuffering();

                    var requestConfigurationBuilder = new ConfigureRequestBuilder(requestType, context);
                    configureRequestDelegate ??= (builder =>
                    {
                        builder.ParseRequestPropertiesFromBody();
                        builder.ParseRequestPropertiesFromQueryParameters();
                        builder.ParseRequestPropertiesFromRouteData();
                    });

                    configureRequestDelegate.Invoke(requestConfigurationBuilder);

                    var response = await context.RequestServices
                        .GetRequiredService<IMediator>()
                        .Send(requestConfigurationBuilder.Request, context.RequestAborted);

                    if (response is Stream streamedResponse)
                    {
                        await streamedResponse.CopyToAsync(context.Response.Body);
                        context.Response.ContentType = metadata.ContentType ?? context.Response.ContentType;
                    }
                    // mediatr will return a fake void type for commands. Check to make sure this is 
                    // not a unit before posting as Json
                    else if (response is not Unit)
                    {
                        await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
                    }

                    await context.Response.Body.FlushAsync(context.RequestAborted);
                });
        }

        return builder;
    }
}
