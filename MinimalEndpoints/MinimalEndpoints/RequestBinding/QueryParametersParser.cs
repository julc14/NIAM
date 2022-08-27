using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MinimalEndpoints.RequestBinding;

internal class QueryParametersParser : IComponentParser
{
    private readonly ILogger<QueryParametersParser> _logger;

    public QueryParametersParser(ILogger<QueryParametersParser> logger) => _logger = logger;

    /// <inheritdoc/>
    public bool TryParse(HttpContext context, PropertyInfo property, [MaybeNullWhen(false)] out object item)
    {
        item = null;

        // IQueryCollection does not throw when key is missing
        // intead it returns an empty string
        // It is not case sensitive
        var values = context.Request.Query[property.Name];

        if (values.Count < 1) 
            return false;
        
        var firstItem = values[0] ?? throw new InvalidOperationException("Unexpected null valued key");

        if (values.Count > 1)
        {
            _logger.LogWarning("Two query values mapped to same key: {PropertyName}. Choosing first: {FirstPropertyValue}",
                property.Name,
                firstItem);
        }

        var convertor = TypeDescriptor.GetConverter(property.PropertyType);
        item = convertor.ConvertFromString(firstItem);

        return item is not null;
    }
}
