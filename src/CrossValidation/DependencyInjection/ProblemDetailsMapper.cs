using System.Net;
using CrossValidation.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CrossValidation.DependencyInjection;

internal class ProblemDetailsMapper(ProblemDetails problemDetails)
{
    public void Map(BusinessException businessException)
    {
        var errors = new List<CrossProblemDetailsError>();
        problemDetails.Status = (int)businessException.StatusCode;
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

        AddExtensions(problemDetails, errors);
    }
    
    public void Map(BusinessListException businessListException)
    {
        var errors = new List<CrossProblemDetailsError>();
        problemDetails.Status = (int)HttpStatusCode.UnprocessableEntity;

        foreach (var error in businessListException.Exceptions)
        {
            errors.Add(CreateCrossProblemDetailsError(error));
        }

        AddExtensions(problemDetails, errors);
    }

    private static void AddExtensions(ProblemDetails problemDetails, List<CrossProblemDetailsError> errors)
    {
        if (errors.Count > 0)
        {
            problemDetails.Extensions.Add("errors", errors);
        }

        // TODO: Add traceId
    }
    
    private static CrossProblemDetailsError CreateCrossProblemDetailsError(BusinessException exception)
    {
        var error = new CrossProblemDetailsError
        {
            Code = exception.Code,
            CodeUrl = null,
            Message = exception.Message == "" ? null : exception.Message,
            Details = exception.Details,
            FieldName = exception.FieldName,
            Placeholders = GetPlaceholders(exception)
        };

        return error;
    }

    private static Dictionary<string, object?>? GetPlaceholders(BusinessException exception)
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
    
    // private string? GetPublicationUrl(BusinessException exception, HttpContext context)
    // {
    //     string? baseUrl = null;
    //     
    //     if (!CrossValidationOptions.IsErrorCodePageEnabled || exception.Code is null)
    //     {
    //         return null;
    //     }
    //
    //     if (CrossValidationOptions.PublicationUrl is not null)
    //     {
    //         baseUrl = CrossValidationOptions.PublicationUrl;
    //     }
    //     else if (_environment.IsDevelopment())
    //     {
    //         var protocol = context.Request.IsHttps ? "https" : "http";
    //         var host = context.Request.Host.Value;
    //         var port = context.Request.Host.Port is not null
    //             ? $":{context.Request.Host.Port.Value}"
    //             : null;
    //         baseUrl = $"{protocol}://{host}{port}";
    //     }
    //     else
    //     {
    //         return null;
    //     }
    //     
    //     return $"{baseUrl}{CrossValidationOptions.ErrorCodePagePath}#{exception.Code}";
    // }
}