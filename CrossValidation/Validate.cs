using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using CrossValidation.Rules;

namespace CrossValidation;

public static class Validate
{
    [Pure]
    public static IRule<TField> That<TField>(TField fieldValue)
    {
        return Rule<TField>.CreateFromField(() => fieldValue, RuleState.Valid);
    }

    [Pure]
    public static IRule<TField> Field<TModel, TField>(
        TModel model,
        Expression<Func<TModel, TField>> fieldSelector)
    {
        return Rule<TField>.CreateFromFieldSelector(model, fieldSelector, RuleState.Valid);
    }
}