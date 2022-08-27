using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace MinimalEndpoints.RequestBinding;

public interface IComponentParser
{
    /// <summary>
    ///     Parse property values out from a source component.
    /// </summary>
    /// <param name="context">
    ///     The Http Context.
    /// </param>
    /// <param name="property">
    ///     The property to parse for.
    /// </param>
    /// <returns>
    ///     Whether the Parse was successful.
    /// </returns>
    ValueTask<object?> ParseAsync(HttpContext context, PropertyInfo property);
}
