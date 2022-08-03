using Refit;

namespace NameItAfterMe.Application.Infrastructure.PictureOfTheDay;

public interface IPictureOfTheDayService
{
    [Get("/planetary/apod")]
    Task<PictureOfTheDayResponse> Get();
}