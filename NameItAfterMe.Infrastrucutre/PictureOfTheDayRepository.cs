using AutoMapper;
using NameItAfterMe.Application.Abstractions;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Infrastructure;

internal class PictureOfTheDayRepository : IPictureOfTheDayRepository
{
    private readonly IPictureOfTheDay _pictureOfTheDay;
    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient;

    public PictureOfTheDayRepository(
        HttpClient httpClient,
        IPictureOfTheDay pictureOfTheDay,
        IMapper mapper)
    {
        (_pictureOfTheDay, _mapper) = (pictureOfTheDay, mapper);
        _httpClient = httpClient;
    }

    /// <summary>
    ///     Return Nasa picture of the day.
    /// </summary>
    /// <returns>
    ///     Picture of the day.
    /// </returns>
    public async Task<PictureOfTheDay> GetPictureOfTheDay()
    {
        var pictureDetails = await _pictureOfTheDay.PictureOfTheDay();
        var pictureOfTheDay = _mapper.Map<PictureOfTheDay>(pictureDetails);

        var response = await _httpClient.GetAsync(pictureOfTheDay.Url);
        pictureOfTheDay.MediaType = response.Content.Headers?.ContentType?.MediaType;

        var contentTypeIsImage =
            pictureOfTheDay.MediaType?.Contains("image", StringComparison.InvariantCultureIgnoreCase) ?? false;

        if (contentTypeIsImage)
            pictureOfTheDay.Content = await _httpClient.GetStreamAsync(pictureOfTheDay.Url);

        return pictureOfTheDay;
    }
}
