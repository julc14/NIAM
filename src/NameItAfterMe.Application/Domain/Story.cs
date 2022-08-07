namespace NameItAfterMe.Application.Domain;

public class Story
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Body { get; set; } = string.Empty;
    public IEnumerable<StoryValue> StoryValues { get; set; }

    public Story(IEnumerable<StoryValue> storyValues, string body)
    {
        StoryValues = storyValues;
        Body = body;
    }

    public Story()
    {
        StoryValues = Enumerable.Empty<StoryValue>();
    }

    public string LoadStory()
    {
        if (StoryValues.Any(x => x.Value is null))
        {
            throw new InvalidOperationException("");
        }

        return string.Format(Body, StoryValues.Select(x => x.Value));
    }
}