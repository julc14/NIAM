using FluentValidation;
using Microsoft.Extensions.Logging;

namespace NameItAfterMe.Server;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger) 
            => (_next, _logger) = (next, logger);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException e)
        {
            context.Response.StatusCode = 422;
            await context.Response.WriteAsJsonAsync(e.Errors);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception. Reporting as server error");

            context.Response.StatusCode = 500;
        }
    }
}