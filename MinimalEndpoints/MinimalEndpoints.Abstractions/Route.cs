using MediatR;
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
        var endpoint = typeof(T).GetCustomAttribute<EndpointAttribute>();

        if (endpoint is null)
        {
            throw new InvalidOperationException($"{typeof(T)} does not have an endpoint attribute");
        }

        // route not defined, use request name as route
        // todo: define default route somewhere else.
        if (endpoint.Route is null)
        {
            return typeof(T).Name;
        }

        var route = endpoint.Route;

        foreach (var property in typeof(T).GetProperties())
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
                route += $"?{property.Name}={value}";
            }
        }

        return route;
    }
}