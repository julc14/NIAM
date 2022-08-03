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

    public GetPictureOfTheDaySourcePathHandler(
        HttpClient httpClient,
        IImageHandler imageHandler,
        IPictureOfTheDayService pictureOfTheDayService)
    {
        _httpClient = httpClient;
        _imageHandler = imageHandler;
        _pictureOfTheDayService = pictureOfTheDayService;
    }

    public async Task<string> Handle(GetPictureOfTheDaySourcePath request, CancellationToken cancellationToken)
    {
        if (_imageHandler.TrySearch("NasaPictureOfTheDay", out var filePath))
        {
            return filePath;
        }

        var pod = await _pictureOfTheDayService.Get();

        var url = request.PreferHd && !string.IsNullOrEmpty(pod.HdUrl)
            ? pod.HdUrl
            : pod.Url;

        if (string.IsNullOrEmpty(url))
            return "Images/DefaultPictureOfTheDay.jpg";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        var mediaType = response.Content.Headers?.ContentType?.MediaType;

        var contentTypeIsImage =
            mediaType?.Contains("image", StringComparison.InvariantCultureIgnoreCase) ?? false;

        if (!contentTypeIsImage)
            return "Images/DefaultPictureOfTheDay.jpg";

        var pictureOfTheDay = await _httpClient.GetStreamAsync(url);

        filePath = await _imageHandler.SaveAsync("NasaPictureOfTheDay", pictureOfTheDay);
        return filePath;
    }
}