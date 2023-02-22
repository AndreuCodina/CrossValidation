using System.Net;
using System.Runtime.CompilerServices;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Validations;

namespace CrossValidation;

public abstract class Ensure<TException> : IValidate<TException>
    where TException : Exception, ICrossErrorToException
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
        return Validate<TException>.That(
            fieldValue,
            error,
            message,
            code,
            details,
            httpStatusCode,
            fieldDisplayName);
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
            allowFieldNameWithoutModel: true,
            error: error,
            message: message,
            code: code,
            details: details,
            httpStatusCode: httpStatusCode,
            fieldDisplayName: fieldDisplayName);
    }

    public static void Must(bool condition, ICrossError error)
    {
        Validate<TException>.Must(condition, error);
    }

    public static void Must(bool condition,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null)
    {
        Validate<TException>.Must(condition, message, code, details, httpStatusCode);
    }
}

public abstract class Ensure : Ensure<CrossArgumentException>
{
}