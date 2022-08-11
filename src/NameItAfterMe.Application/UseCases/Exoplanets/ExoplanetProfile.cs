using AutoMapper;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.UseCases.Exoplanets;

internal class ExoplanetProfile : Profile
{
    public ExoplanetProfile()
    {
        CreateMap<Exoplanet, ExoplanetDto>()
            .ForMember(x => x.Distance, y => y.MapFrom(x => x.Distance.Value + x.Distance.Unit));
    }
}
