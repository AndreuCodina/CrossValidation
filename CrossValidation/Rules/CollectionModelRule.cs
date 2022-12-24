using System.Collections;
using System.Linq.Expressions;
using CrossValidation.Utils;
using CrossValidation.ValidationContexts;

namespace CrossValidation.Rules;

public class CollectionModelRule<TModel, TField> : ModelRule<TModel, TField>
{
    internal CollectionModelRule(
        ModelValidationContext context,
        TModel model,
        string fieldFullPath,
        TField? fieldValue,
        int? index = null,
        string? parentPath = null) :
        base(context, model, fieldFullPath, fieldValue, index, parentPath)
    {
    }
    
    internal static CollectionModelRule<TModel, TField> Create<TField>(
        TModel model,
        ModelValidationContext context,
        Expression<Func<TModel, TField?>> fieldSelector)
        where TField : IEnumerable
    {
        var fieldInformation = Util.GetFieldInformation(fieldSelector, model);
        return new CollectionModelRule<TModel, TField>(
            context,
            model,
            fieldInformation.SelectionFullPath,
            fieldInformation.Value);
    }
}