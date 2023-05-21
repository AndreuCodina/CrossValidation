using System.Net;
using System.Runtime.CompilerServices;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Utils;
using CrossValidation.Validations;

namespace CrossValidation;

public abstract class Validate<TException>
    where TException : Exception
{
    public static IValidation<TField> That<TField>(
        TField field,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null)
    {
        return new Validation<TField>(
            getFieldValue: () => field,
            crossErrorToException: typeof(TException),
            generalizeError: true,
            fieldPath: null,
            context: null,
            index: null,
            parentPath: null,
            fixedError: error,
            fixedMessage: message,
            fixedCode: code,
            fixedDetails: details,
            fixedHttpStatusCode: httpStatusCode,
            fixedFieldDisplayName: fieldDisplayName);
    }
    
    public static IValidation<TField> Field<TField>(
        TField field,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null,
        [CallerArgumentExpression(nameof(field))] string fieldName = default!)
    {
        return IValidation<TField>.CreateFromFieldName(
            getFieldValue: () => field,
            crossErrorToException: typeof(TException),
            fieldName: fieldName,
            context: null,
            error: error,
            message: message,
            code: code,
            details: details,
            httpStatusCode: httpStatusCode,
            fieldDisplayName: fieldDisplayName);
    }

    public static void Must(bool condition, ICrossError error)
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
    
    private static void InternalMust(
        bool condition,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null)
    {
        if (!condition)
        {
            var validation = That(
                field: condition,
                error: null,
                message: null,
                code: null,
                details: null,
                httpStatusCode: null,
                fieldDisplayName: null);
        
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
    
    public static IValidation<TField> Argument<TField>(
        TField field,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null,
        [CallerArgumentExpression(nameof(field))] string fieldName = default!)
    {
        return IValidation<TField>.CreateFromFieldName(
            getFieldValue: () => field,
            crossErrorToException: typeof(TException) == typeof(CrossException)
                ? typeof(ArgumentException)
                : typeof(TException),
            fieldName,
            context: null,
            error: error,
            message: message,
            code: code,
            details: details,
            httpStatusCode: httpStatusCode,
            fieldDisplayName: fieldDisplayName);
    }

    public static void ModelNullability<TModel>(TModel model)
    {
        ModelNullabilityValidator.Validate(model);
    }
}

public abstract class Validate : Validate<CrossException>
{
}