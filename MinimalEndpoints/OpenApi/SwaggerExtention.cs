using Microsoft.Extensions.DependencyInjection;
using NameItAfterMe.Server.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MinimalEndpoints.OpenApi;

public static class SwaggerExtention
{
    public static SwaggerGenOptions AddMinimalEndpointSupport(this SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.OperationFilter<ParameterFilter>();
        return swaggerGenOptions;
    }
}
