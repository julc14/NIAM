using Refit;

namespace NameItAfterMe.Application.Infrastructure.Nasa.PictureOfTheDay;

public interface IPictureOfTheDayService
{
    [Get("/planetary/apod")]
    Task<PictureOfTheDayResponse> Get();
}