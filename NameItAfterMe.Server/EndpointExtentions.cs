using MediatR;
using NameItAfterMe.Application;
using System.Reflection;

namespace NameItAfterMe.Server;

public static class EndpointExtentions
{
    public static IEndpointRouteBuilder MapUseCasesFromAssembly(
        this IEndpointRouteBuilder builder,
        Assembly assembly,
        Action<IConfigureRequestBinding>? configureRequestDelegate = null)
    {
        var useCasesToHost = assembly
            .GetTypes()
            .Select(x => (request: x, metadata: x.GetCustomAttribute<WebHostedUseCaseAttribute>()))
            .Where(x => x.metadata is not null);

        foreach (var (requestType, metadata) in useCasesToHost)
        {
            builder.Map(
                metadata!.Route ?? requestType.Name,
                async context =>
                {
                    // ensure the body can be read multiple times
                    // this may be unnessary for this project but can't be sure all middleware will not attempt to 
                    // read the request body.
                    context.Request.EnableBuffering();

                    var requestConfigurationBuilder = new ConfigureRequestBuilder(requestType, context);
                    configureRequestDelegate ??= (builder => builder.ParseRequestPropertiesFromBody());

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
