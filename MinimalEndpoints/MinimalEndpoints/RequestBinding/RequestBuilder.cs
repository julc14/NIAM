using MediatR;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace MinimalEndpoints.RequestBinding;

internal class RequestBuilder
{
    private readonly IEnumerable<IComponentParser> _componentParsers;
    private object? _requestBodyDeserialized;

    public RequestBuilder(
        IEnumerable<IComponentParser> componentParsers) => _componentParsers = componentParsers;

    /// <summary>
    ///     Parses an object of a given type from the Http context. Priority given to component parsers added last.
    /// </summary>
    /// <param name="requestType">
    ///     The request type to parse for.
    /// </param>
    /// <param name="context">
    ///     The http context.
    /// </param>
    /// <returns>
    ///     The parsed object.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     When the request type does not have a public parameterless constructor.
    /// </exception>
    public async ValueTask<object> Build(Type requestType, HttpContext context)
    {
        // todo: almost certainly more performant to cache the ctor delegate instead of using activator.
        var request = Activator.CreateInstance(requestType)
            ?? throw new InvalidOperationException($"{requestType} does not have a public parameterless constructor.");

        var propertiesWithAccessibleSetters =
            requestType.GetProperties().Where(x => x.GetSetMethod() is not null);

        foreach (var property in propertiesWithAccessibleSetters)
        {
            await SetPropertyValue(context, property, request);
        }

        return request;
    }

    private async ValueTask SetPropertyValue(HttpContext context, PropertyInfo property, object request)
    {
        foreach (var component in _componentParsers)
        {
            var parsedValue = await component.ParseAsync(context, property);

            if (parsedValue is not null)
                property.SetValue(request, parsedValue);
        }

        _requestBodyDeserialized ??= ShouldParseRequestBody(context.Request)
            ? await context.Request.ReadFromJsonAsync(request.GetType())
            : null;

        // todo: perf test heavy use of reflection, optimize
        if (_requestBodyDeserialized is not null)
        {
            var requestBodyPropertyValue = property.GetValue(_requestBodyDeserialized);

            if (requestBodyPropertyValue is not null or 0 or "")
            {
                property.SetValue(request, requestBodyPropertyValue);
            }
        }
    }

    private static bool ShouldParseRequestBody(HttpRequest request)
        => request.ContentLength is not null or 0 && request.HasJsonContentType();
}