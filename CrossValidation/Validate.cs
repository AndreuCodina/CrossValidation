using System.Linq.Expressions;
using CrossValidation.Rules;

namespace CrossValidation;

public static class Validate
{
    public static InlineRule<TField> That<TField>(TField fieldValue)
    {
        return new InlineRule<TField>(fieldValue);
    }
    
    public static InlineRule<TField> Field<TModel, TField>(
        TModel model,
        Expression<Func<TModel, TField>> fieldSelector)
    {
        return InlineRule<TField>.CreateFromSelector(model, fieldSelector);
    }
}