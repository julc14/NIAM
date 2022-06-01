using MediatR;
using Microsoft.AspNetCore.Mvc;
using NameItAfterMe.Application.UseCases;

namespace NameItAfterMe.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PictureOfTheDayController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromServices] IMediator mediator)
        {
            var fileStream = await mediator.Send(new GetPictureOfTheDayStream());
            return File(fileStream, "image/jpg");
        }
    }
}