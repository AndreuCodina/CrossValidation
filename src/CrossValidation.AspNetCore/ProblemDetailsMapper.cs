using System.Net;
using CrossValidation.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CrossValidation.AspNetCore;

internal class ProblemDetailsMapper(ProblemDetails problemDetails)
{
    public void Map(BusinessException businessException)
    {
        SetStatusDetails(problemDetails, businessException.StatusCode);
        var errors = new List<CrossValidationProblemDetailsError>();
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
        SetStatusDetails(problemDetails, (int)HttpStatusCode.UnprocessableEntity);
        var errors = new List<CrossValidationProblemDetailsError>();

        foreach (var error in businessListException.Exceptions)
        {
            errors.Add(CreateCrossProblemDetailsError(error));
        }

        AddExtensions(problemDetails, errors);
    }

    private static void AddExtensions(ProblemDetails problemDetails, List<CrossValidationProblemDetailsError> errors)
    {
        if (errors.Count > 0)
        {
            problemDetails.Extensions.Add("errors", errors);
        }
    }
    
    private CrossValidationProblemDetailsError CreateCrossProblemDetailsError(BusinessException exception)
    {
        var error = new CrossValidationProblemDetailsError
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

    private void SetStatusDetails(ProblemDetails problemDetails, int statusCode)
    {
        CrossValidationProblemDetailsDefaults.Apply(problemDetails, statusCode);
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