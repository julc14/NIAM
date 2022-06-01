using MediatR;
using NameItAfterMe.Application.Abstractions;
using System.Reflection;

namespace NameItAfterMe.Application.UseCases.PictureOfTheDay;

[WebHostedUseCase(
    ContentType = "image/jpg",
    Route = "PictureOfTheDay")]
public class GetPictureOfTheDayStream : IRequest<Stream>
{
    public bool IsHd { get; set; } = true;
}

public class GetPictureOfTheDayStreamHandler : IRequestHandler<GetPictureOfTheDayStream, Stream>
{
    private readonly HttpClient _httpClient;
    private readonly IPictureOfTheDayRepository _pictureOfTheDayRepository;

    public GetPictureOfTheDayStreamHandler(
        HttpClient httpClient,
        IPictureOfTheDayRepository pictureOfTheDayRepository)
            => (_httpClient, _pictureOfTheDayRepository)
            = (httpClient, pictureOfTheDayRepository);

    public async Task<Stream> Handle(GetPictureOfTheDayStream request, CancellationToken cancellationToken)
    {
        var picOfDay = await
            _pictureOfTheDayRepository.GetPictureOfTheDay().ConfigureAwait(false);

        var imageGet = await _httpClient.GetAsync(picOfDay.Url, cancellationToken).ConfigureAwait(false);
        var mediaType = imageGet.Content.Headers?.ContentType?.MediaType;

        if (mediaType?.Contains("image", StringComparison.InvariantCultureIgnoreCase) == true)
        {
            return await _httpClient.GetStreamAsync(picOfDay.Url, cancellationToken).ConfigureAwait(false);
        }

        return Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("NameItAfterMe.Application.UseCases.PictureOfTheDay.BaseBackground.jpg")
          ?? throw new InvalidOperationException();
    }
}
