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
        return IValidation<TField>.CreateFromFieldName(
            getFieldValue: () => field,
            crossErrorToException: typeof(TException),
            fieldName: fieldName,
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
        throw new NotImplementedException();
        // if (!condition)
        // {
        //     var validation = IValidation<bool>.CreateFromField(() => condition, typeof(TException));
        //
        //     if (error is not null)
        //     {
        //         validation = validation.WithError(error);
        //     }
        //
        //     if (message is not null)
        //     {
        //         validation = validation.WithMessage(message);
        //     }
        //
        //     if (code is not null)
        //     {
        //         validation = validation.WithCode(code);
        //     }
        //
        //     if (details is not null)
        //     {
        //         validation = validation.WithDetails(details);
        //     }
        //
        //     if (httpStatusCode is not null)
        //     {
        //         validation = validation.WithHttpStatusCode(httpStatusCode.Value);
        //     }
        //
        //     validation.Must(_ => false);
        // }
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
            () => field,
            typeof(TException) == typeof(CrossException)
                ? typeof(ArgumentException)
                : typeof(TException),
            fieldName,
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