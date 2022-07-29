using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace MinimalEndpoints.RequestBinding;

internal class RequestBodyParser : IComponentParser
{
    /// <inheritdoc/>
    public bool TryParse(HttpContext context, PropertyInfo property, [MaybeNullWhen(false)] out object propertyValue)
    {
        propertyValue = null;

        if (context.Request.ContentLength is null
            || context.Request.ContentLength == 0
            || !context.Request.HasJsonContentType())
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(context.Request.Body);

            if (doc.RootElement.TryGetProperty(property.Name, out var propertyValueToken))
            {
                propertyValue = propertyValueToken.Deserialize(property.PropertyType);
            }
        }
        catch (JsonException)
        {
        }

        return propertyValue is not null;
    }
}
