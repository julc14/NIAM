namespace NameItAfterMe.Application;

[AttributeUsage(AttributeTargets.Class)]
public class WebHostedUseCaseAttribute : Attribute
{
    public string? Route { get; init; }
    public string? ContentType { get; init; }
}
