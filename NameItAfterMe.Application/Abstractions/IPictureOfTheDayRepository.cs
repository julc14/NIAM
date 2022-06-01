using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.Abstractions;

public interface IPictureOfTheDayRepository
{
    Task<PictureOfTheDay> GetPictureOfTheDay();
}
