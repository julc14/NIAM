using Microsoft.Extensions.DependencyInjection;
using NameItAfterMe.Server.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MinimalEndpoints.OpenApi;

public static class SwaggerExtention
{
    /// <summary>
    ///     Add swagger support for minimal endpoints.
    /// </summary>
    /// <param name="swaggerGenOptions">
    ///     The options.
    /// </param>
    /// <returns>
    ///     The options.
    /// </returns>
    public static SwaggerGenOptions AddMinimalEndpointSupport(this SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.OperationFilter<ParameterFilter>();
        return swaggerGenOptions;
    }
}
