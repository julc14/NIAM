using AutoMapper;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Infrastructure;

internal class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<PictureOfTheDayResponse, PictureOfTheDay>()
            .ForMember(
                x => x.Url,
                x => x.MapFrom(y => string.IsNullOrWhiteSpace(y.HdUrl)
                    ? y.Url
                    : y.HdUrl));

        CreateMap<ExoplanetQueryResponse, Exoplanet>();
    }
}
