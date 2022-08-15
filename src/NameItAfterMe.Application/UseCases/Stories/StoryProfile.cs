using AutoMapper;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.UseCases.Stories;

internal class StoryProfile : Profile
{
	public StoryProfile()
	{
		CreateMap<Story, StoryDto>()
			.ForMember(x => x.WordDescriptor, y => y.MapFrom(x => x.WordDescriptor.Select(x => x.Descriptor)));
	}
}
