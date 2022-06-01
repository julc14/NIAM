using AutoMapper;
using NameItAfterMe.Application.Abstractions;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Infrastructure;

internal class PictureOfTheDayRepository : IPictureOfTheDayRepository
{
    private readonly IPictureOfTheDay _pictureOfTheDay;
    private readonly IMapper _mapper;

    public PictureOfTheDayRepository(
        IPictureOfTheDay pictureOfTheDay,
        IMapper mapper)
            => (_pictureOfTheDay, _mapper) = (pictureOfTheDay, mapper);

    /// <summary>
    ///     Return Nasa picture of the day.
    /// </summary>
    /// <returns>
    ///     Picture of the day.
    /// </returns>
    public async Task<PictureOfTheDay> GetPictureOfTheDay()
    {
        var pictureDetails = await _pictureOfTheDay.PictureOfTheDay().ConfigureAwait(false);

        return _mapper.Map<PictureOfTheDay>(pictureDetails);
    }
}
