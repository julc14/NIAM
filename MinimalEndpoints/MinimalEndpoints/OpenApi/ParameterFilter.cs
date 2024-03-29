﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MinimalEndpoints.OpenApi;

/// <summary>
///     Manually add mediatr request properties as swagger optional parameters.
///     Swagger will not see these otherwise since we have manually bound asp request => mediatr request.
///     This has no effect on runtime behavior, just swagger.
/// </summary>
public class ParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpointDescriptor = context.ApiDescription.ActionDescriptor;

        var metadata = endpointDescriptor.EndpointMetadata.OfType<EndpointMetadata>().FirstOrDefault();
        if (metadata is null)
            return;

        foreach (var property in metadata.RequestType.GetProperties())
        {
            var bindingSource = ParameterLocation.Query;
            var path = context.ApiDescription.RelativePath;

            if (path is not null && path.Contains(property.Name, StringComparison.OrdinalIgnoreCase))
            {
                bindingSource = ParameterLocation.Path;
            }

            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = property.Name,
                In = bindingSource,
                Required = false
            });
        }
    }
}
