using System.Net;
using System.Runtime.CompilerServices;
using CrossValidation.Exceptions;
using CrossValidation.Utils;
using CrossValidation.Validations;

namespace CrossValidation;

public abstract class Validate
{
    public static IValidation<TField> That<TField>(
        TField field,
        BusinessException? exception = null,
        string message = "",
        string? code = null,
        string? details = null,
        HttpStatusCode? statusCode = null,
        string? fieldDisplayName = null)
    {
        return new Validation<TField>(
            getFieldValue: () => field,
            customThrowToThrow: null,
            createGenericError: true,
            fieldPath: null,
            context: null,
            index: null,
            parentPath: null,
            fixedException: exception,
            fixedMessage: message,
            fixedCode: code,
            fixedDetails: details,
            fixedStatusCode: statusCode,
            fixedFieldDisplayName: fieldDisplayName);
    }
    
    public static IValidation<TField> Field<TField>(
        TField field,
        BusinessException? exception = null,
        string message = "",
        string? code = null,
        string? details = null,
        HttpStatusCode? statusCode = null,
        string? fieldDisplayName = null,
        [CallerArgumentExpression(nameof(field))] string fieldName = default!)
    {
        return IValidation<TField>.CreateFromFieldName(
            getFieldValue: () => field,
            customExceptionToThrow: null,
            fieldName: fieldName,
            context: null,
            exception: exception,
            message: message,
            code: code,
            details: details,
            statusCode: statusCode,
            fieldDisplayName: fieldDisplayName);
    }

    public static void Must(bool condition, BusinessException exception)
    {
        InternalMust(condition, exception: exception);
    }
    
    public static void Must(
        bool condition,
        string message = "",
        string? code = null,
        string? details = null,
        HttpStatusCode? statusCode = null)
    {
        InternalMust(
            condition,
            message: message,
            code: code,
            details: details,
            statusCode: statusCode);
    }
    
    private static void InternalMust(
        bool condition,
        BusinessException? exception = null,
        string message = "",
        string? code = null,
        string? details = null,
        HttpStatusCode? statusCode = null)
    {
        if (!condition)
        {
            var validation = That(
                field: condition,
                exception: null,
                message: "",
                code: null,
                details: null,
                statusCode: null,
                fieldDisplayName: null);
        
            if (exception is not null)
            {
                validation = validation.WithException(exception);
            }
        
            if (message != "")
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
        
            if (statusCode is not null)
            {
                validation = validation.WithHttpStatusCode(statusCode.Value);
            }
        
            validation.Must(_ => false);
        }
    }
    
    public static IValidation<TField> Argument<TField>(
        TField field,
        BusinessException? exception = null,
        string message = "",
        string? code = null,
        string? details = null,
        HttpStatusCode? statusCode = null,
        string? fieldDisplayName = null,
        [CallerArgumentExpression(nameof(field))] string fieldName = default!)
    {
        return Validate<ArgumentException>.Argument(
            field: field,
            exception: exception,
            message: message,
            code: code,
            details: details,
            statusCode: statusCode,
            fieldDisplayName: fieldDisplayName,
            fieldName: fieldName);
    }

    public static void ModelNullability<TModel>(TModel model)
    {
        ModelNullabilityValidator.Validate(model);
    }
}

public abstract class Validate<TException>
    where TException : Exception
{
    public static IValidation<TField> Argument<TField>(
        TField field,
        BusinessException? exception = null,
        string message = "",
        string? code = null,
        string? details = null,
        HttpStatusCode? statusCode = null,
        string? fieldDisplayName = null,
        [CallerArgumentExpression(nameof(field))] string fieldName = default!)
    {
        return IValidation<TField>.CreateFromFieldName(
            getFieldValue: () => field,
            customExceptionToThrow: typeof(TException),
            fieldName,
            context: null,
            exception: exception,
            message: message,
            code: code,
            details: details,
            statusCode: statusCode,
            fieldDisplayName: fieldDisplayName);
    }
}