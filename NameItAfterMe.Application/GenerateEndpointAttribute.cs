namespace NameItAfterMe.Application;

[AttributeUsage(AttributeTargets.Class)]
public class GenerateEndpointAttribute : Attribute
{
    public string? Route { get; init; }
    public string? ContentType { get; init; }
}
