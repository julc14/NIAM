namespace NameItAfterMe.Application.Domain;

public class Story
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Body { get; set; } = string.Empty;
    
    public string Format(IEnumerable<string> values)
    {
        return string.Format(Body, values);
    }
}