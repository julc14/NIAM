using MediatR;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MinimalEndpoints.Abstractions;

// todo: this needs revised before its ready.
// todo: perf concerns, cache properties + attributes ?
// todo: handle requets with non-trivial properties.
public static class RoutingExtentions
{
    /// <summary>
    ///     Project a medaitr request to an HttpRoute
    /// </summary>
    /// <typeparam name="T">
    ///     The type of mediatr request to project.
    /// </typeparam>
    /// <param name="request">
    ///     The request to project.
    /// </param>
    /// <returns>
    ///     A string representing an http route.
    /// </returns>
    public static string AsRoute<T>(this T request)
        where T : IBaseRequest
    {
        var concreteType = request.GetType();
        var endpoint = concreteType.GetCustomAttribute<EndpointAttribute>();

        if (endpoint is null)
        {
            throw new InvalidOperationException($"{concreteType} does not have an endpoint attribute");
        }

        // route not defined, use request name as route
        // todo: define default route somewhere else.
        if (endpoint.Route is null)
        {
            return concreteType.Name;
        }

        var routeBuilder = new StringBuilder(endpoint.Route);
        var firstQueryParam = true;

        foreach (var property in concreteType.GetProperties())
        {
            var value = property.GetValue(request);

            if (value == default)
                continue;

            var match = Regex.Match(endpoint.Route, $@"{{({property.Name}.*?)\}}");

            if (match.Success)
            {
                // route param
                routeBuilder.Replace(match.Value, value.ToString());
            }
            else
            {
                // query param
                var leadingChar = firstQueryParam
                    ? '?'
                    : '&';

                firstQueryParam = false;
                routeBuilder.Append(leadingChar).Append(property.Name).Append('=').Append(value);
            }
        }

        return routeBuilder.ToString();
    }

    /// <summary>
    ///     Sends a GET request to a uri constructed from the request and returns
    ///     the value that results from deserialzing the body as JSON in an asyncronous operation.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of request.
    /// </typeparam>
    /// <param name="client">
    ///     The Http client.
    /// </param>
    /// <param name="request">
    ///     The medaitr request.
    /// </param>
    /// <param name="token">
    ///     The cancellation token, if any
    /// </param>
    /// <returns>
    ///     A Task representing an asyncronous operation.
    /// </returns>
    public static Task<T?> GetFromJsonAsync<T>(
        this HttpClient client,
        IRequest<T> request,
        CancellationToken token = default)
    {
        return client.GetFromJsonAsync<T>(request.AsRoute(), token);
    }
}