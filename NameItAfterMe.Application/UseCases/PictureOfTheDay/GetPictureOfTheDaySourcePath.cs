using MediatR;
using MinimalEndpoints;
using NameItAfterMe.Application.Infrastructure.Files;
using NameItAfterMe.Application.Infrastructure.PictureOfTheDay;

namespace NameItAfterMe.Application.UseCases.PictureOfTheDay;

[Endpoint(Route = "PictureOfTheDay/SourcePath")]
public class GetPictureOfTheDaySourcePath : IRequest<string>
{
    public bool PreferHd { get; set; } = true;
}

public class GetPictureOfTheDaySourcePathHandler : IRequestHandler<GetPictureOfTheDaySourcePath, string>
{
    private readonly HttpClient _httpClient;
    private readonly IImageHandler _imageHandler;
    private readonly IPictureOfTheDayService _pictureOfTheDayService;

    private const string DefaultPictureOfTheDayPath = "Images/DefaultPictureOfTheDay.jpg";
    private const string BaseImageTitle = "NasaPictureOfTheDay";

    public GetPictureOfTheDaySourcePathHandler(
        HttpClient httpClient,
        IImageHandler imageHandler,
        IPictureOfTheDayService pictureOfTheDayService)
            => (_httpClient, _imageHandler, _pictureOfTheDayService)
            = (httpClient, imageHandler, pictureOfTheDayService);

    public async Task<string> Handle(GetPictureOfTheDaySourcePath request, CancellationToken cancellationToken)
    {
        if (_imageHandler.TrySearch(BaseImageTitle, out var image))
        {
            return image.LocalRootPath;
        }

        var pod = await _pictureOfTheDayService.Get();

        var url = request.PreferHd && !string.IsNullOrEmpty(pod.HdUrl)
            ? pod.HdUrl
            : pod.Url;

        if (string.IsNullOrEmpty(url))
        {
            return DefaultPictureOfTheDayPath;
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        var mediaType = response.Content.Headers?.ContentType?.MediaType;

        var contentTypeIsImage =
            mediaType?.Contains("image", StringComparison.InvariantCultureIgnoreCase) ?? false;

        // sometimes pic of day is actually a video
        if (!contentTypeIsImage)
        {
            return DefaultPictureOfTheDayPath;
        }

        image = await _imageHandler.SaveAsync(BaseImageTitle, () => _httpClient.GetStreamAsync(url));

        return image.LocalRootPath;
    }
}