﻿namespace NameItAfterMe.Blazor.Client.Components.SubmissionOverlay;

public class NameExoplanetOperationState
{
    public string? PersonsName { get; set; }
    public NamingScheme Scheme { get; set; }
    public string? StoryName { get; set; }
    public required string ExoplanetName { get; set; }
    public IEnumerable<WordDescriptor> WordDescriptors { get; set; } = Enumerable.Empty<WordDescriptor>();
    public string? StoryBody { get; set; }
}

public class WordDescriptor
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}
