namespace MinimalEndpoints;

public class EndpointMetadata
{
    public required Type RequestType { get; init; }
    public required Type ResponseType { get; init; }
    public required string Route { get; init; }
    public required HttpMethods HttpMethod { get; init; }
    public string? ContentType { get; init; }
}
