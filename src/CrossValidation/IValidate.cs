using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.CompilerServices;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Validations;

namespace CrossValidation;

internal interface IValidate<TException>
    where TException : Exception, ICrossErrorToException
{
    [Pure]
    static abstract IValidation<TField> That<TField>(
        TField fieldValue,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null);

    [Pure]
    static abstract IValidation<TField> Field<TField>(
        TField field,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null,
        [CallerArgumentExpression(nameof(field))]
        string fieldName = default!);

    static abstract void Must(bool condition, ICrossError error);

    static abstract void Must(
        bool condition,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null);

    internal static void InternalMust(bool condition,
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
}