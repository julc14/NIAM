using MediatR;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MinimalEndpoints.Abstractions;

// todo: this needs revised before its ready.
// todo: perf concerns, cache properties ?
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
    public static string AsRoute<T>(this T request) where T : IBaseRequest
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

        var route = endpoint.Route;
        var firstQueryParam = true;

        foreach (var property in concreteType.GetProperties())
        {

            var value = property.GetValue(request);

            if (value == default)
                continue;

            var match = Regex.Match(route, $@"{{({property.Name}.*?)\}}");

            if (match.Success)
            {
                // route param
                route = route.Replace(match.Value, value.ToString());
            }
            else
            {

                // query param
                if (firstQueryParam)
                {
                    route += $"?{property.Name}={value}";
                    firstQueryParam = false;
                }
                else
                {
                    route += $"&{property.Name}={value}";
                }
            }
        }

        return route;
    }

    public static Task<T?> GetFromJsonAsync<T>(this HttpClient client, IRequest<T> request, CancellationToken token = default)
    {
        return client.GetFromJsonAsync<T>(request.AsRoute(), token);
    }
}