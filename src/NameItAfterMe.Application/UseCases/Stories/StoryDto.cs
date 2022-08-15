using AutoMapper;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.UseCases.Stories;

public record StoryDto
{
    public string Name { get; set; }
    public IEnumerable<string> WordDescriptor { get; set; }
}
