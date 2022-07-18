//using MediatR;
//using MinimalEndpoints;
//using NameItAfterMe.Application.Abstractions;
//using HttpMethod = MinimalEndpoints.HttpMethod;

//namespace NameItAfterMe.Application.UseCases.PictureOfTheDay;

//[Endpoint(nameof(HttpMethod.Get))]
//public class GetPictureOfTheDay : IRequest<Stream>
//{

//}

//public class GetPictureOfTheDayHandler : IRequestHandler<GetPictureOfTheDay, Stream>
//{
//    private readonly IImageHandler _imageHandler;
//    private readonly IPictureOfTheDayRepository _pictureOfTheDayRepository;

//    public GetPictureOfTheDayHandler(
//        IImageHandler imageHandler,
//        IPictureOfTheDayRepository pictureOfTheDayRepository)
//    {
//        _imageHandler = imageHandler;
//        _pictureOfTheDayRepository = pictureOfTheDayRepository;
//    }

//    public async Task<Stream> Handle(GetPictureOfTheDay request, CancellationToken cancellationToken)
//    {
//        if (_imageHandler.TryRead("NasaPictureOfTheDay", out var fileStream))
//        {
//            return fileStream;
//        }

//        var picOfDay = await _pictureOfTheDayRepository.GetPictureOfTheDay();

//        if (picOfDay.Content is null)
//        {
//            if (_imageHandler.TryRead("DefaultPictureOfTheDay", out var defaultStream))
//            {
//                return defaultStream;
//            }

//            throw new InvalidOperationException("Cannot find any image of the day");
//        }

//        // fire and forget.
//        _ = _imageHandler.SaveAsync("NasaPictureOfTheDay", picOfDay.Content);

//        return picOfDay.Content;
//    }
//}