using FluentValidation;

namespace NameItAfterMe.Server;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

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
        catch (Exception)
        {
            context.Response.StatusCode = 500;
        }
    }
}
