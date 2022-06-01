using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace NameItAfterMe.Application.Behavior;

internal class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => 
        _logger = logger;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        var requestName = typeof(TRequest).Name;
        var requestJson = JsonSerializer.Serialize(request);
        var id = Guid.NewGuid();

        _logger.LogInformation(
            "Unique Request Id: {RequestId}, Request name:{RequestName}, request json:{RequestJson}", id, requestName, requestJson);

        var timer = new Stopwatch();
        timer.Start();

        var response = await next().ConfigureAwait(false);

        timer.Stop();

        _logger.LogInformation(
            "End Unique Request Id: {RequestId}, Request name:{RequestName}, total request time:{ElapsedMilliseconds}ms", id, requestName, timer.ElapsedMilliseconds);

        return response;
    }
}
