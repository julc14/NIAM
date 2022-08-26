using MediatR;
using MinimalEndpoints;
using NameItAfterMe.Application.Infrastructure.Files;
using NameItAfterMe.Application.Infrastructure.PictureOfTheDay;

namespace NameItAfterMe.Application.UseCases.PictureOfTheDay;

[Endpoint(Route = "PictureOfTheDay/SourcePath")]
public class GetPictureOfTheDaySourcePath : IRequest<string>
{
    public bool PreferHd { get; set; } = true;
    public bool ReturnDefaultImageOnError { get; set; } = true;
}

public class GetPictureOfTheDaySourcePathHandler : IRequestHandler<GetPictureOfTheDaySourcePath, string>
{
    private readonly HttpClient _httpClient;
    private readonly IImageHandler<PictureOfTheDayImage> _imageHandler;
    private readonly IPictureOfTheDayService _pictureOfTheDayService;

    private const string DefaultPictureOfTheDayPath = "Common/DefaultPictureOfTheDay.jpg";

    public GetPictureOfTheDaySourcePathHandler(
        HttpClient httpClient,
        IImageHandler<PictureOfTheDayImage> imageHandler,
        IPictureOfTheDayService pictureOfTheDayService)
    {
        _httpClient = httpClient;
        _imageHandler = imageHandler;
        _pictureOfTheDayService = pictureOfTheDayService;
    }

    public async Task<string> Handle(GetPictureOfTheDaySourcePath request, CancellationToken cancellationToken)
    {
        var imageOfTheDay = await _imageHandler
            .EnumerateImagesAsync(cancellationToken)
            .FirstOrDefaultAsync(x => (DateTime.UtcNow - x.CreatedOn).Hours <= 24, cancellationToken);

        if (imageOfTheDay is not null)
        {
            return imageOfTheDay.Url;
        }

        var pod = await _pictureOfTheDayService.Get();

        var url = request.PreferHd && !string.IsNullOrEmpty(pod.HdUrl)
            ? pod.HdUrl
            : pod.Url;

        if (request.ReturnDefaultImageOnError && string.IsNullOrEmpty(url))
        {
            return DefaultPictureOfTheDayPath;
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (request.ReturnDefaultImageOnError && !ContentTypeIsImage(response))
        {
            return DefaultPictureOfTheDayPath;
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        imageOfTheDay = await _imageHandler.UploadAsync(stream, token: cancellationToken);

        return imageOfTheDay.Url;
    }

    private static bool ContentTypeIsImage(HttpResponseMessage response)
    {
        var mediaType = response.Content.Headers?.ContentType?.MediaType;
        return mediaType?.Contains("image", StringComparison.InvariantCultureIgnoreCase) ?? false;
    }
}