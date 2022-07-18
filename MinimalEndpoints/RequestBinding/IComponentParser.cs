using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
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
    /// <param name="propertyValue">
    ///     The parsed value, if any.
    /// </param>
    /// <returns>
    ///     Whether the Parse was successful.
    /// </returns>
    bool TryParse(HttpContext context, PropertyInfo property, [MaybeNullWhen(false)] out object? propertyValue);
}
