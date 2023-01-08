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

    public static void Is(bool condition)
    {
        InternalIs(condition);
    }

    public static void Is(bool condition, CrossValidationError error)
    {
        InternalIs(condition, error: error);
    }
    
    public static void Is(
        bool condition,
        string code,
        string? message = null,
        string? details = null)
    {
        InternalIs(
            condition,
            code: code,
            message: message,
            details: details);
    }

    private static void InternalIs(
        bool condition,
        CrossValidationError? error = null,
        string? code = null,
        string? message = null,
        string? details = null)
    {
        if (!condition)
        {
            var rule = IValidRule<bool>.CreateFromField(condition);

            if (error is not null)
            {
                rule = rule.WithError(error);
            }
            
            if (code is not null)
            {
                rule = rule.WithCode(code);
            }
            
            if (message is not null)
            {
                rule = rule.WithMessage(message);
            }
            
            if (details is not null)
            {
                rule = rule.WithDetails(details);
            }

            rule.Must(_ => false);
        }
    }
}