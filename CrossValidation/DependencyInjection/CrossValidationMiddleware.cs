using System.Diagnostics;
using System.Net;
using System.Text.Json;
using CrossValidation.Exceptions;
using CrossValidation.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CrossValidation.DependencyInjection;

internal class CrossValidationMiddleware : IMiddleware
{
    private readonly ILogger<CrossValidationMiddleware> _logger;
    private readonly IHostingEnvironment _environment;

    public CrossValidationMiddleware(ILoggerFactory loggerFactory, IHostingEnvironment environment)
    {
        _logger = loggerFactory.CreateLogger<CrossValidationMiddleware>();
        _environment = environment;
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            await HandleException(e, context);
        }
    }

    private async Task HandleException(Exception exception, HttpContext context)
    {
        var problemDetails = new CrossProblemDetails();
        var genericError = "An error occurred";
        var httpStatusCode = HttpStatusCode.InternalServerError;
        string? title = null;
        string? details = null;

        if (exception is ValidationException or ValidationException)
        {
            httpStatusCode = HttpStatusCode.UnprocessableEntity;
            title = "One validation errors occurred";
        }
        else if (exception is ValidationException or ValidationListException)
        {
            httpStatusCode = HttpStatusCode.UnprocessableEntity;
            title = "Several validation errors occurred";
        }
        else if (exception is EnsureException {Error: ModelNullabilityValidatorError error})
        {
            httpStatusCode = HttpStatusCode.BadRequest;
            title = "Nullability error";

            if (error is ModelNullabilityValidatorError.NonNullablePropertyIsNullError err)
            {
                details = err.ToString();
            }
            else if (error is ModelNullabilityValidatorError.NonNullablePropertyIsNullError errr)
            {
                details = errr.ToString();
            }
            else
            {
                throw new UnreachableException();
            }
        }
        else if (exception is EnsureException ensureException)
        {
            httpStatusCode = HttpStatusCode.UnprocessableEntity;
            title = genericError;
        }
        else
        {
            _logger.LogError(exception, exception.Message);
            title = genericError;
            problemDetails.Detail = _environment.IsDevelopment() ? exception.Message : null;
        }

        problemDetails.Status = (int)httpStatusCode;
        problemDetails.Title = title;
        problemDetails.Detail = details;
        var response = JsonSerializer.Serialize(problemDetails);
        context.Response.StatusCode = problemDetails.Status.Value;
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers.Add("X-Trace-Id", context.Request.Headers["X-Trace-Id"]);
        await context.Response.WriteAsync(response);
    }
}