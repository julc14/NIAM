namespace MinimalEndpoints;

public class EndpointMetadata
{
    public Type RequestType { get; }
    public Type ResponseType { get; }
    public string Route { get; }
    public HttpMethods HttpMethod { get; init; }
    public string? ContentType { get; init; }

    public EndpointMetadata(
        Type requestType,
        Type responseType,
        string route,
        HttpMethods? httpMethod = null)
    {
        RequestType = requestType;
        ResponseType = responseType;
        Route = route;

        HttpMethod = httpMethod
            ?? InferHttpMethodFromRequestType(requestType)
            ?? throw new InvalidOperationException(
                $"{ nameof(httpMethod) } not provided and cannot be inferred from request type");
    }

    private static HttpMethods? InferHttpMethodFromRequestType(Type requestType)
    {
        var requestTypeName = requestType.Name;

        var httpMethod = Enum
            .GetNames<HttpMethods>()
            .FirstOrDefault(httpMethod => requestTypeName.StartsWith(httpMethod));

        if (httpMethod is null)
            return null;

        return Enum.Parse<HttpMethods>(httpMethod);
    }
}
