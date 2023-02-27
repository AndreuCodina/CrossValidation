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
        TField fieldValue,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null)
    {
        return IValidValidation<TField>.CreateFromField(
            fieldValue,
            typeof(TException),
            error: error,
            message: message,
            code: code,
            details: details,
            httpStatusCode: httpStatusCode,
            fieldDisplayName: fieldDisplayName);
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
        return IValidValidation<TField>.CreateFromFieldName(
            field,
            typeof(TException),
            fieldName,
            allowFieldNameWithoutModel: false,
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
    
    private static void InternalMust(bool condition,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null)
    {
        if (!condition)
        {
            var validation = IValidValidation<bool>.CreateFromField(condition, typeof(TException));

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
        return IValidValidation<TField>.CreateFromFieldName(
            field,
            typeof(TException) == typeof(CrossException)
                ? typeof(ArgumentException)
                : typeof(TException),
            fieldName,
            allowFieldNameWithoutModel: true,
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