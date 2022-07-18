using Microsoft.AspNetCore.Http;

namespace MinimalEndpoints.RequestBinding;

internal class RequestBuilder
{
    private readonly IEnumerable<IComponentParser> _componentParsers;

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
    public object Build(Type requestType, HttpContext context)
    {
        var request = Activator.CreateInstance(requestType)
            ?? throw new InvalidOperationException($"{requestType} does not have a public paramaterless constructor.");

        foreach (var property in requestType.GetProperties())
        foreach (var component in _componentParsers)
        {
            if (property.GetSetMethod() is not null
                && component.TryParse(context, property, out var propertyValue))
            {
                property.SetValue(request, propertyValue);
            }
        }

        return request;
    }
}