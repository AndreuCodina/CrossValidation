using CrossValidation.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CrossValidation.DependencyInjection;

internal class GlobalExceptionMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GlobalExceptionMiddleware>();
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            if (e is not BusinessException or BusinessListException)
            {
                HandleUnexpectedException(e, context);
                // throw;
            }

            // context.Response.StatusCode = 404;
            throw;
        }
    }

    private void HandleUnexpectedException(Exception exception, HttpContext context)
    {
        if (!CrossValidationOptions.HandleUnknownException)
        {
            return;
        }

        _logger.LogError(exception, exception.Message);
    }
}