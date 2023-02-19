using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.CompilerServices;
using CrossValidation.Errors;
using CrossValidation.Utils;
using CrossValidation.Validations;

namespace CrossValidation;

public static class Validate
{
    [Pure]
    public static IValidation<TField> That<TField>(TField fieldValue)
    {
        return IValidValidation<TField>.CreateFromField(fieldValue);
    }
    
    [Pure]
    public static IValidation<TField> Field<TField>(
        TField field,
        [CallerArgumentExpression(nameof(field))] string fieldName = "")
    {
        return IValidValidation<TField>.CreateFromFieldName(field, fieldName);
    }

    public static void Must(bool condition, CrossError error)
    {
        InternalMust(condition, error: error);
    }
    
    public static void Must(
        bool condition,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null)
    {
        InternalMust(
            condition,
            message: message,
            code: code,
            details: details,
            httpStatusCode: httpStatusCode);
    }

    private static void InternalMust(bool condition,
        CrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null)
    {
        if (!condition)
        {
            var validation = IValidValidation<bool>.CreateFromField(condition);

            if (error is not null)
            {
                validation = validation.WithError(error);
            }
            
            if (message is not null)
            {
                validation = validation.WithMessage(message);
            }
            
            if (code is not null)
            {
                validation = validation.WithCode(code);
            }

            if (details is not null)
            {
                validation = validation.WithDetails(details);
            }
            
            if (httpStatusCode is not null)
            {
                validation = validation.WithHttpStatusCode(httpStatusCode.Value);
            }

            validation.Must(_ => false);
        }
    }

    public static void ModelNullability<TModel>(TModel model)
    {
        ModelNullabilityValidator.Validate(model);
    }
}