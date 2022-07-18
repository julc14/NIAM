using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MinimalEndpoints.RequestBinding;

public interface IComponentParser
{
    bool TryParse(HttpContext context, PropertyInfo property, [MaybeNullWhen(false)] out object? propertyValue);
}
