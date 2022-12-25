using System.Linq.Expressions;
using CrossValidation.Rules;

namespace CrossValidation;

public static class Validate
{
    public static InlineRule<TField> That<TField>(TField fieldValue)
    {
        return InlineRule<TField>.CreateFromField(fieldValue);
    }

    public static InlineRule<TField> Field<TModel, TField>(
        TModel model,
        Expression<Func<TModel, TField?>> fieldSelector)
    {
        return InlineRule<TField>.CreateFromFieldSelector(model, fieldSelector);
    }
}