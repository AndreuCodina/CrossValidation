using CrossValidation.Validators;

namespace CrossValidation.Rules;

public static class ModelRuleExtensions
{
    public static ModelRule<TModel, TField> NotNull<TModel, TField>(
        this ModelRule<TModel, TField?> rule)
        where TField : class
    {
        return rule.SetValidator(new NotNullValidator<TField?>(rule.FieldValue))!;
    }
    
    public static ModelRule<TModel, TField> NotNull<TModel, TField>(
        this ModelRule<TModel, TField?> rule)
        where TField : struct
    {
        return rule.SetValidator(new NotNullValidator<TField?>(rule.FieldValue))
            .Transform(x => x!.Value);
    }
    
    public static ModelRule<TModel, TField?> Null<TModel, TField>(
        this ModelRule<TModel, TField?> rule)
        where TField : class
    {
        return rule.SetValidator(new NullValidator<TField?>(rule.FieldValue));
    }
    
    public static ModelRule<TModel, TField?> Null<TModel, TField>(
        this ModelRule<TModel, TField?> rule)
        where TField : struct
    {
        return rule.SetValidator(new NullValidator<TField?>(rule.FieldValue));
    }
    
    public static CollectionModelRule<TModel, IEnumerable<TInnerType>> ForEach<TModel, TInnerType>(
        this CollectionModelRule<TModel, IEnumerable<TInnerType>> rule,
        Action<ModelRule<TModel, TInnerType>> action)
    {
        var fieldCollection = rule.FieldValue!;
        var fieldFullPath = rule.Context.FieldName;
        var index = 0;
        
        foreach (var innerField in fieldCollection)
        {
            var newRule = new ModelRule<TModel, TInnerType>(
                rule.Context,
                rule.Model,
                fieldFullPath: fieldFullPath,
                innerField,
                index,
                parentPath: rule.Context.ParentPath);
            action(newRule);
            index++;
        }

        return rule;
    }
}