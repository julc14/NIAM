namespace MinimalEndpoints;

[AttributeUsage(AttributeTargets.Class)]
public class EndpointAttribute : Attribute
{
    public HttpMethods? HttpMethod { get; init; }
    public string? Route { get; init; }
    public string? ContentType { get; init; }

    public EndpointAttribute(HttpMethods httpMethod) => HttpMethod = httpMethod;
    public EndpointAttribute() { }
}