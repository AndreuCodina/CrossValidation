using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using CrossValidation.Errors;
using CrossValidation.Rules;

namespace CrossValidation;

public static class Validate
{
    [Pure]
    public static IRule<TField> That<TField>(TField fieldValue)
    {
        return IValidRule<TField>.CreateFromField(fieldValue);
    }

    [Pure]
    public static IRule<TField> Field<TModel, TField>(
        TModel model,
        Expression<Func<TModel, TField>> fieldSelector)
    {
        return IValidRule<TField>.CreateFromFieldSelector(model, fieldSelector);
    }

    public static void Must(bool condition, ValidationError error)
    {
        InternalMust(condition, error: error);
    }
    
    public static void Must(
        bool condition,
        string? message = null,
        string? code = null,
        string? details = null)
    {
        InternalMust(
            condition,
            message: message,
            code: code,
            details: details);
    }

    private static void InternalMust(
        bool condition,
        ValidationError? error = null,
        string? message = null,
        string? code = null,
        string? details = null)
    {
        if (!condition)
        {
            var rule = IValidRule<bool>.CreateFromField(condition);

            if (error is not null)
            {
                rule = rule.WithError(error);
            }
            
            if (message is not null)
            {
                rule = rule.WithMessage(message);
            }
            
            if (code is not null)
            {
                rule = rule.WithCode(code);
            }

            if (details is not null)
            {
                rule = rule.WithDetails(details);
            }

            rule.Must(_ => false);
        }
    }
}