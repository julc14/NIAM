using MediatR;
using Microsoft.Extensions.Logging;
using NameItAfterMe.Application.Abstractions;
using System.Reflection;

namespace NameItAfterMe.Application.UseCases.PictureOfTheDay;

[GenerateEndpoint(ContentType = "image/jpg")]
public class GetPictureOfTheDay : IRequest<Stream>
{
    public bool IsHd { get; set; } = true;
    public bool GetDefaultAsFallback { get; set; } = true;
}

public class GetPictureOfTheDayHandler : IRequestHandler<GetPictureOfTheDay, Stream>
{
    private readonly HttpClient _httpClient;
    private readonly IPictureOfTheDayRepository _pictureOfTheDayRepository;
    private readonly ILogger _logger;

    public GetPictureOfTheDayHandler(
        HttpClient httpClient,
        ILogger<GetPictureOfTheDayHandler> logger,
        IPictureOfTheDayRepository pictureOfTheDayRepository)
            => (_httpClient, _pictureOfTheDayRepository, _logger)
            = (httpClient, pictureOfTheDayRepository, logger);

    public async Task<Stream> Handle(GetPictureOfTheDay request, CancellationToken cancellationToken)
    {
        var picOfDay = await _pictureOfTheDayRepository
            .GetPictureOfTheDay()
            .ConfigureAwait(false);

        var imageGet = await _httpClient
            .GetAsync(picOfDay.Url, cancellationToken)
            .ConfigureAwait(false);

        var mediaType = imageGet.Content.Headers?.ContentType?.MediaType;
        var contentTypeIsImage =
            mediaType?.Contains("image", StringComparison.InvariantCultureIgnoreCase) == true;

        if (contentTypeIsImage)
            return await _httpClient.GetStreamAsync(picOfDay.Url, cancellationToken).ConfigureAwait(false);

        if (!request.GetDefaultAsFallback)
        {
            _logger.LogWarning("Source content is not an image and no default options is selected. Returning empty stream");
            return Stream.Null;
        }

        var defaultImageStream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("NameItAfterMe.Application.UseCases.PictureOfTheDay.BaseBackground.jpg");

        if (defaultImageStream is null)
            throw new FileNotFoundException();

        return defaultImageStream;
    }
}
