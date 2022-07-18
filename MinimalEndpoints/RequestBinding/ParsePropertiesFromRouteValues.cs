using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MinimalEndpoints.RequestBinding;

public class ParsePropertiesFromRouteValues : IComponentParser
{
    public bool TryParse(HttpContext context, PropertyInfo property, [MaybeNullWhen(false)] out object? propertyValue)
    {
        propertyValue = null;

        // RouteValueDictionary does not throw when key is missing
        // intead it returns null
        // It is not case sensitive
        var value = context.Request.RouteValues[property.Name];

        if (value is not null)
        {
            var valueAsString = value.ToString();

            if (valueAsString is not null)
            {
                var convertor = TypeDescriptor.GetConverter(property.PropertyType);
                propertyValue = convertor.ConvertFromString(valueAsString);
            }
        }

        return propertyValue is not null;
    }
}
