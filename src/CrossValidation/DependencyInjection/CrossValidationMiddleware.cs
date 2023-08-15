using System.Net;
using System.Text.Json;
using CrossValidation.Exceptions;
using CrossValidation.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CrossValidation.DependencyInjection;

public class CrossValidationMiddleware : IMiddleware
{
    private readonly ILogger<CrossValidationMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public CrossValidationMiddleware(ILoggerFactory loggerFactory, IWebHostEnvironment environment)
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

            // if (_isExceptionNotHandled)
            // {
            //     throw; // Se we can add another exception middleware after this one
            // }
        }
    }

    private async Task HandleException(Exception exception, HttpContext context)
    {
        var problemDetails = new CrossProblemDetails();
        HttpStatusCode? statusCode = null;
        string? type = null;
        string? title = null;
        string? detail = null;
        List<CrossProblemDetailsError> errors = new();
        string? exceptionDetail = null;
        
        if (exception is BusinessException businessException)
        {
            statusCode = businessException.StatusCode;
            title = "A validation error occurred";
            var error = CreateCrossProblemDetailsError(businessException, context);
            
            var allErrorCustomizationsAreNotSet =
                error is
                {
                    Code: null,
                    Message: null,
                    Detail: null,
                    Placeholders: null
                };

            if (!allErrorCustomizationsAreNotSet)
            {
                errors.Add(error);
            }
        }
        else if (exception is BusinessListException validationListException)
        {
            statusCode = HttpStatusCode.UnprocessableEntity;
            title = "Several validation errors occurred";

            foreach (var error in validationListException.Exceptions)
            {
                errors.Add(CreateCrossProblemDetailsError(error, context));
            }
        }
        else if (exception is NonNullablePropertyIsNullException nonNullablePropertyIsNullException)
        {
            statusCode = HttpStatusCode.BadRequest;
            title = "Nullability error";
            detail = $"Non nullable property is null: {nonNullablePropertyIsNullException.PropertyName}";
        }
        else if (exception is NonNullableItemCollectionWithNullItemException
                 nonNullableItemCollectionWithNullItemException)
        {
            statusCode = HttpStatusCode.BadRequest;
            title = "Nullability error";
            detail = $"Non nullable item collection with null item: {nonNullableItemCollectionWithNullItemException.CollectionName}";
        }
        else
        {
            if (!CrossValidationOptions.HandleUnknownException)
            {
                return;
            }
            
            _logger.LogError(exception, exception.Message);
            statusCode = HttpStatusCode.InternalServerError;
            type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1";
            title = "An error occurred";
            detail = _environment.IsDevelopment() ? exception.Message : null;
            exceptionDetail = _environment.IsDevelopment() ? exception.ToString() : null;
        }

        problemDetails.Status = (int)statusCode;
        problemDetails.Type = type;
        problemDetails.Title = title;
        problemDetails.Detail = detail;
        problemDetails.Errors = errors.Any() ? errors : null;
        problemDetails.ExceptionDetail = exceptionDetail;
        var response = JsonSerializer.Serialize(problemDetails);
        context.Response.StatusCode = problemDetails.Status.Value;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(response);
    }

    private CrossProblemDetailsError CreateCrossProblemDetailsError(BusinessException exception, HttpContext context)
    {
        var error = new CrossProblemDetailsError
        {
            Code = exception.Code,
            CodeUrl = GetPublicationUrl(exception, context),
            Message = exception.Message == "" ? null : exception.Message,
            Detail = exception.Details,
            FieldName = exception.FieldName,
            Placeholders = GetPlaceholders(exception)
        };

        return error;
    }

    private string? GetPublicationUrl(BusinessException exception, HttpContext context)
    {
        string? baseUrl = null;
        
        if (!CrossValidationOptions.IsErrorCodePageEnabled || exception.Code is null)
        {
            return null;
        }

        if (CrossValidationOptions.PublicationUrl is not null)
        {
            baseUrl = CrossValidationOptions.PublicationUrl;
        }
        else if (_environment.IsDevelopment())
        {
            var protocol = context.Request.IsHttps ? "https" : "http";
            var host = context.Request.Host.Value;
            var port = context.Request.Host.Port is not null
                ? $":{context.Request.Host.Port.Value}"
                : null;
            baseUrl = $"{protocol}://{host}{port}";
        }
        else
        {
            return null;
        }
        
        return $"{baseUrl}{CrossValidationOptions.ErrorCodePagePath}#{exception.Code}";
    }

    private Dictionary<string, object?>? GetPlaceholders(BusinessException exception)
    {
        var hasToSendPlaceholders =
            CrossValidationOptions.LocalizeCommonErrorsInFront
            && exception.PlaceholderValues.Count > 0;

        if (exception is not FrontBusinessException && !hasToSendPlaceholders)
        {
            return null;
        }
        
        var placeholders = new Dictionary<string, object?>();

        foreach (var placeholder in exception.PlaceholderValues)
        {
            placeholders.Add(placeholder.Key, placeholder.Value);
        }

        return placeholders;
    }
}