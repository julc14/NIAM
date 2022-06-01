using MediatR;
using NameItAfterMe.Application;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace NameItAfterMe.Server;

public static class EndpointGeneration
{
    private static readonly JsonSerializerOptions _serializationOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async ValueTask MapEndpoint(
        Type requestType,
        HttpContext context,
        WebHostedUseCaseAttribute metadata)
    {
        var endpointDelegate = typeof(EndpointGeneration)
             .GetMethod(nameof(MapEndpoint), BindingFlags.Static | BindingFlags.NonPublic)
            ?.MakeGenericMethod(requestType)
           ?? throw new InvalidOperationException($"Failed to locate method {nameof(MapEndpoint)}");

        var methodParameters = new object[] { context, metadata };
        var endpointDelegateTask = endpointDelegate.Invoke(null, methodParameters) as Task;

        await endpointDelegateTask;
    }

    private static async Task MapEndpoint<TRequest>(
        HttpContext context,
        WebHostedUseCaseAttribute metadata)
    {
        var request = context.Request.ContentLength > 0
            ? await BindRequestFromBody<TRequest>(context)
            : Activator.CreateInstance<TRequest>()
          ?? throw new InvalidOperationException();

        request = await BindRequestFromQueryParameters(context, request);
        request = await BindRequestFromRouteData(context, request);

        // Do we need to create a scope here or can we assume the host has already created one?
        var mediatr = context.RequestServices.GetRequiredService<IMediator>();

        try
        {
            var response = await mediatr.Send(request, context.RequestAborted);
            context.Response.StatusCode = 200;

            if (response is Stream streamedResponse)
            {
                await streamedResponse.CopyToAsync(context.Response.Body);

                if (metadata.ContentType != null)
                {
                    context.Response.ContentType = metadata.ContentType;
                }
            }
            // mediatr will return a fake void type for commands. Check to make sure this is 
            // not a unit before posting as Json
            else if (response is not Unit)
            {
                await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
            }

            await context.Response.Body.FlushAsync(context.RequestAborted);
        }
        catch
        {
            context.Response.StatusCode = 404;
        }
    }

    private static async ValueTask<TRequest> BindRequestFromBody<TRequest>(
        HttpContext context) =>
        await JsonSerializer.DeserializeAsync<TRequest>(
            context.Request.Body,
            _serializationOptions,
            context.RequestAborted)
        ?? throw new InvalidOperationException("failed to create request from request body");

    private static ValueTask<TRequest> BindRequestFromQueryParameters<TRequest>(
        HttpContext context,
        TRequest request)
    {
        var queryProperties =
            from requestProperty in typeof(TRequest).GetProperties()

            join query in context.Request.Query
                on requestProperty.Name.ToLower() equals query.Key.ToLower()

            // check what happens when type cant find convertor.
            let typeConvertor = TypeDescriptor.GetConverter(requestProperty.PropertyType)
            let conversionAttempt = typeConvertor.ConvertFromString(query.Value.ToString())

            // check if type conversion issues being hidden here
            where conversionAttempt is not null

            select new
            {
                requestProperty,
                typedObjectValue = conversionAttempt
            };

        foreach (var item in queryProperties)
        {
            item.requestProperty.SetValue(request, item.typedObjectValue);
        }

        return ValueTask.FromResult(request);
    }

    private static ValueTask<TRequest> BindRequestFromRouteData<TRequest>(
        HttpContext context,
        TRequest request)
    {
        var routeData = context.GetRouteData().Values;

        var routeDataProperties =
            from requestProperty in typeof(TRequest).GetProperties()

            join route in routeData
                on requestProperty.Name.ToLower() equals route.Key.ToLower()

            // check what happens when type cant find convertor.
            let typeConvertor = TypeDescriptor.GetConverter(requestProperty.PropertyType)
            let conversionAttempt = typeConvertor.ConvertFromString(route.Value.ToString())

            // check if type conversion issues being hidden here
            where conversionAttempt is not null

            select new
            {
                requestProperty,
                typedObjectValue = conversionAttempt
            };

        foreach (var item in routeDataProperties)
        {
            item.requestProperty.SetValue(request, item.typedObjectValue);
        }

        return ValueTask.FromResult(request);
    }
}
