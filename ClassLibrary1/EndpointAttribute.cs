namespace MinimalEndpoints;

[AttributeUsage(AttributeTargets.Class)]
public class EndpointAttribute : Attribute
{
    private static readonly string[] _canNotHaveBody = new string[]
    {
        "Get",
        "Head"
    };

    public string HttpMethod { get; }
    public string? Route { get; init; }
    public string? ContentType { get; init; }

    public EndpointAttribute(string httpMethod) => HttpMethod = httpMethod;

    public bool CanHaveBody => !_canNotHaveBody.Contains(HttpMethod, StringComparer.OrdinalIgnoreCase);
}