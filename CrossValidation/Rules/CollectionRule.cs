using System.Collections;
using System.Linq.Expressions;
using CrossValidation.Utils;
using CrossValidation.ValidationContexts;

namespace CrossValidation.Rules;

public class CollectionRule<TField> : Rule<TField>
    where TField : IEnumerable
{
    private CollectionRule(
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null)
        : base(fieldValue, fieldFullPath, context, index, parentPath)
    {
    }

    public static CollectionRule<TField> CreateFromFieldSelector<TModel>(
        TModel model,
        ValidationContext context,
        Expression<Func<TModel, TField>> fieldSelector)
    {
        var fieldInformation = Util.GetFieldInformation(fieldSelector, model);
        return new CollectionRule<TField>(
            fieldInformation.Value,
            fieldInformation.SelectionFullPath,
            context);
    }

}