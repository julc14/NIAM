namespace MinimalEndpoints;

public class EndpointMetadata
{
    public Type RequestType { get; }
    public Type ResponseType { get; }
    public string Route { get; }
    public string HttpMethod { get; }
    public string? ContentType { get; init; }

    public EndpointMetadata(
        Type requestType,
        Type responseType,
        string route,
        string httpMethod)
    {
        RequestType = requestType;
        ResponseType = responseType;
        Route = route;
        HttpMethod = httpMethod;
    }
}
