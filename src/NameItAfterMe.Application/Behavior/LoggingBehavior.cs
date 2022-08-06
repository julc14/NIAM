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
        var id = Guid.NewGuid();

        Log.RequestCreated(
            _logger,
            id,
            requestName: typeof(TRequest).Name,
            requestJson: JsonSerializer.Serialize(request));

        var timer = new Stopwatch();
        timer.Start();

        var response = await next();

        timer.Stop();

        Log.RequestCompleted(
            _logger,
            id,
            requestName: typeof(TRequest).Name,
            timer.ElapsedMilliseconds);

        return response;
    }
}

internal static partial class Log
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Unique Request Id: {id}, Request name:{requestName}, request json:{requestJson}")]
    public static partial void RequestCreated(ILogger logger, Guid id, string requestName, string requestJson);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "End Unique Request Id: {id}, Request name:{requestName}, total request time:{elapsedMilliseconds}ms")]
    public static partial void RequestCompleted(ILogger logger, Guid id, string requestName, long elapsedMilliseconds);
}