using System.Net;
using System.Runtime.CompilerServices;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Utils;
using CrossValidation.Validations;

namespace CrossValidation;

public abstract class Validate<TException> : IValidate<TException>
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
        IValidate<TException>.InternalMust(condition, error: error);
    }
    
    public static void Must(
        bool condition,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null)
    {
        IValidate<TException>.InternalMust(
            condition,
            message: message,
            code: code,
            details: details,
            httpStatusCode: httpStatusCode);
    }
    
    public static void ModelNullability<TModel>(TModel model)
    {
        ModelNullabilityValidator.Validate(model);
    }
}

public abstract class Validate : Validate<CrossException>
{
}