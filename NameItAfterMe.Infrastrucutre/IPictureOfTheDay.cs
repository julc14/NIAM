using Refit;

namespace NameItAfterMe.Infrastructure;

public interface IPictureOfTheDay
{
    [Get("/planetary/apod")]
    Task<PictureOfTheDayResponse> PictureOfTheDay();
}