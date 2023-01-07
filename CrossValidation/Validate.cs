using System.Diagnostics.Contracts;
using System.Linq.Expressions;
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
}