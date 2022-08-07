using MediatR;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MinimalEndpoints.Abstractions;

public static class Routing<T> where T : IBaseRequest
{
    public static EndpointAttribute Endpoint =
        typeof(T).GetCustomAttribute<EndpointAttribute>() ?? throw new InvalidOperationException("");
}

public static class RoutingExtentions
{
    public static string AsRoute<T>(this T request) where T : IBaseRequest
    {
        var route = Routing<T>.Endpoint.Route ?? typeof(T).Name;

        foreach (var property in typeof(T).GetProperties())
        {
            var value = property.GetValue(request);

            if (value == default)
                continue;

            var match = Regex.Match(route, $@"{{({property.Name}.*?)\}}");

            if (match.Success)
            {
                route = route.Replace(match.Value, value.ToString());
            }
            else
            {
                route += $"?{property.Name}={value}";
            }
        }

        return route;
    }
}