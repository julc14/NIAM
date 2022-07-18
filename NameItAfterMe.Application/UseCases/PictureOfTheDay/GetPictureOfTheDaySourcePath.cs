using MediatR;
using MinimalEndpoints;
using NameItAfterMe.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMethod = MinimalEndpoints.HttpMethod;

namespace NameItAfterMe.Application.UseCases.PictureOfTheDay;

[Endpoint(nameof(HttpMethod.Get), Route = "PictureOfTheDay/SourcePath")]
public class GetPictureOfTheDaySourcePath : IRequest<string>
{
}

public class GetPictureOfTheDaySourcePathHandler : IRequestHandler<GetPictureOfTheDaySourcePath, string>
{
    private readonly IImageHandler _imageHandler;
    private readonly IPictureOfTheDayRepository _pictureOfTheDayRepository;

    public GetPictureOfTheDaySourcePathHandler(
        IImageHandler imageHandler,
        IPictureOfTheDayRepository pictureOfTheDayRepository)
    {
        _imageHandler = imageHandler;
        _pictureOfTheDayRepository = pictureOfTheDayRepository;
    }

    public async Task<string> Handle(GetPictureOfTheDaySourcePath request, CancellationToken cancellationToken)
    {
        if (_imageHandler.TrySearch("NasaPictureOfTheDay", out var filePath))
        {
            return filePath;
        }

        var picOfDay = await _pictureOfTheDayRepository.GetPictureOfTheDay();

        if (picOfDay.Content is null)
        {
            return "Images/DefaultPictureOfTheDay.jpg";
        }

        // fire and forget.
        filePath = await _imageHandler.SaveAsync("NasaPictureOfTheDay", picOfDay.Content);

        return filePath;
    }
}