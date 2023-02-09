using System.Net;
using System.Text.Json;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CrossValidation.DependencyInjection;

public class CrossValidationMiddleware : IMiddleware
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
        var httpStatusCode = HttpStatusCode.InternalServerError;
        string? title = null;
        string? details = null;
        List<Dictionary<string, object>> errors = new();

        if (exception is CrossException crossException)
        {
            httpStatusCode = HttpStatusCode.UnprocessableEntity;
            title = "One validation errors occurred";
            var error = CreateCrossProblemDetailsError(crossException.Error);
            errors.Add(error);
        }
        else if (exception is ValidationListException validationListException)
        {
            httpStatusCode = HttpStatusCode.UnprocessableEntity;
            title = "Several validation errors occurred";

            foreach (var error in validationListException.Errors)
            {
                errors.Add(CreateCrossProblemDetailsError(error));
            }
        }
        else if (exception is NonNullablePropertyIsNullException nonNullablePropertyIsNullException)
        {
            httpStatusCode = HttpStatusCode.BadRequest;
            title = "Nullability error";
            details = $"Non nullable property is null: {nonNullablePropertyIsNullException.PropertyName}";
        }
        else if (exception is NonNullableItemCollectionWithNullItemException
                 nonNullableItemCollectionWithNullItemException)
        {
            httpStatusCode = HttpStatusCode.BadRequest;
            title = "Nullability error";
            details = $"Non nullable item collection with null item: {nonNullableItemCollectionWithNullItemException.CollectionName}";
        }
        else
        {
            _logger.LogError(exception, exception.Message);
            title = "An error occurred";;
            problemDetails.Detail = _environment.IsDevelopment() ? exception.Message : null;
        }

        problemDetails.Status = (int)httpStatusCode;
        problemDetails.Title = title;
        problemDetails.Detail = details;
        problemDetails.Errors = errors.Any() ? errors : null;
        var response = JsonSerializer.Serialize(problemDetails);
        context.Response.StatusCode = problemDetails.Status.Value;
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers.Add("X-Trace-Id", context.Request.Headers["X-Trace-Id"]);
        await context.Response.WriteAsync(response);
    }

    private Dictionary<string, object> CreateCrossProblemDetailsError(ICrossError crossError)
    {
        var error = new Dictionary<string, object>();
        
        if (crossError.Code is not null)
        {
            error.Add(
                nameof(crossError.Code),
                crossError.Code);
        }
        
        if (crossError.Message is not null)
        {
            error.Add(
                nameof(crossError.Message),
                crossError.Message);
        }
            
        if (crossError.Details is not null)
        {
            error.Add(
                nameof(crossError.Details),
                crossError.Details);
        }

        foreach (var placeholder in crossError.PlaceholderValues!)
        {
            error.Add(placeholder.Key, placeholder.Value);
        }

        return error;
    }
}