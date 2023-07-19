using System.Net;
using System.Text.Json;
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
        var statusCode = HttpStatusCode.InternalServerError;
        string? type = null;
        string? title = null;
        string? details = null;
        List<CrossProblemDetailsError> errors = new();
        
        if (exception is BusinessException businessException)
        {
            statusCode = businessException.StatusCode;
            title = "A validation error occurred";
            var error = CreateCrossProblemDetailsError(businessException);
            
            var allErrorCustomizationsAreNotSet =
                error is
                {
                    Code: null,
                    Message: null,
                    Details: null,
                    Placeholders: null
                };

            if (!allErrorCustomizationsAreNotSet)
            {
                errors.Add(error);
            }
        }
        else if (exception is ValidationListException validationListException)
        {
            statusCode = HttpStatusCode.UnprocessableEntity;
            title = "Several validation errors occurred";

            foreach (var error in validationListException.Exceptions)
            {
                errors.Add(CreateCrossProblemDetailsError(error));
            }
        }
        else if (exception is NonNullablePropertyIsNullException nonNullablePropertyIsNullException)
        {
            statusCode = HttpStatusCode.BadRequest;
            title = "Nullability error";
            details = $"Non nullable property is null: {nonNullablePropertyIsNullException.PropertyName}";
        }
        else if (exception is NonNullableItemCollectionWithNullItemException
                 nonNullableItemCollectionWithNullItemException)
        {
            statusCode = HttpStatusCode.BadRequest;
            title = "Nullability error";
            details = $"Non nullable item collection with null item: {nonNullableItemCollectionWithNullItemException.CollectionName}";
        }
        else
        {
            if (!CrossValidationOptions.HandleUnknownException)
            {
                return;
            }
            
            _logger.LogError(exception, exception.Message);
            type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1";
            title = "An error occurred";
            problemDetails.Detail = _environment.IsDevelopment() ? exception.Message : null;
        }

        problemDetails.Status = (int)statusCode;
        problemDetails.Type = type;
        problemDetails.Title = title;
        problemDetails.Detail = details;
        problemDetails.Errors = errors.Any() ? errors : null;
        var response = JsonSerializer.Serialize(problemDetails);
        context.Response.StatusCode = problemDetails.Status.Value;
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers.Add("X-Trace-Id", context.Request.Headers["X-Trace-Id"]);
        await context.Response.WriteAsync(response);
    }

    private CrossProblemDetailsError CreateCrossProblemDetailsError(BusinessException exception)
    {
        var error = new CrossProblemDetailsError
        {
            Code = exception.Code,
            Message = exception.Message == "" ? null : exception.Message,
            Details = exception.Details
        };

        if (exception is FrontBusinessException
            || (CrossValidationOptions.LocalizeCommonErrorsInFront
                && exception.PlaceholderValues.Count > 0))
        {
            var placeholders = new Dictionary<string, object?>();
            
            foreach (var placeholder in exception.PlaceholderValues)
            {
                placeholders.Add(placeholder.Key, placeholder.Value);
            }

            error.Placeholders = placeholders;
        }

        return error;
    }
}