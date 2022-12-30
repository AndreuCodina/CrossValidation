using System.Linq.Expressions;
using CrossValidation.Rules;

namespace CrossValidation;

public static class Validate
{
    public static Rule<TField> That<TField>(TField fieldValue)
    {
        return Rule<TField>.CreateFromField(fieldValue);
    }

    public static Rule<TField> Field<TModel, TField>(
        TModel model,
        Expression<Func<TModel, TField?>> fieldSelector)
    {
        return Rule<TField>.CreateFromFieldSelector(model, fieldSelector);
    }
}