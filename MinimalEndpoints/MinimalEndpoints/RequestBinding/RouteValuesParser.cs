using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.Reflection;

namespace MinimalEndpoints.RequestBinding;

public class RouteValuesParser : IComponentParser
{
    public ValueTask<object?> ParseAsync(HttpContext context, PropertyInfo property)
    {
        // RouteValueDictionary does not throw when key is missing
        // instead it returns null
        // It is not case sensitive
        var value = context.Request.RouteValues[property.Name];

        var valueAsString = value?.ToString();

        if (valueAsString is null)
            return ValueTask.FromResult<object?>(null);

        var convertor = TypeDescriptor.GetConverter(property.PropertyType);

        return ValueTask.FromResult(convertor.ConvertFromString(valueAsString));
    }
}
