using CrossValidation.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CrossValidation.AspNetCore;

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
            HandleUnexpectedException(e, context);
            throw;
        }
    }

    private void HandleUnexpectedException(Exception exception, HttpContext context)
    {
        if (CrossValidationOptions.HandleUnknownException
            && exception is BusinessException or BusinessListException)
        {
            return;
        }

        _logger.LogError(exception, exception.Message);
    }
}