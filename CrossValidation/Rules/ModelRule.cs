using System.Linq.Expressions;
using CrossValidation.Results;
using CrossValidation.Utils;
using CrossValidation.ValidationContexts;

namespace CrossValidation.Rules;

public class ModelRule<TModel, TField>
    : Rule<
        ModelRule<TModel, TField>,
        TField,
        ModelValidationContext
    >
{
    public TModel Model { get; set; }
    public string FieldFullPath { get; set; }
    public Expression<Func<TModel, TField>> FieldSelector { get; set; }

    private ModelRule(
        ModelValidationContext context,
        TModel model,
        string fieldFullPath,
        TField fieldValue,
        bool isFieldSelectedDifferentThanModel,
        Expression<Func<TModel, TField>> fieldSelector)
    {
        Context = context;
        Model = model;
        FieldFullPath = fieldFullPath;
        var fieldValueToAssign = isFieldSelectedDifferentThanModel ? fieldValue : default;
        FieldValue = fieldValueToAssign;
        Context.FieldValue = fieldValueToAssign;
        var parentPath = context.ParentPath is not null ? context.ParentPath + "." : "";
        Context.FieldName = parentPath + fieldFullPath;
        FieldSelector = fieldSelector;
    }
    
    public static ModelRule<TModel, TField> Create(
        TModel model,
        ModelValidationContext context,
        Expression<Func<TModel, TField>> fieldSelector)
    {
        var fieldInformation = Util.GetFieldInformation(fieldSelector, model);
        return new ModelRule<TModel, TField>(
            context,
            model,
            fieldInformation.FieldFullPath,
            fieldInformation.FieldValue,
            fieldInformation.IsFieldSelectedDifferentThanModel,
            fieldSelector)
        {
            Context = context,
            Model = model,
            FieldFullPath = fieldInformation.FieldFullPath,
        };
    }
    
    public ModelRule<TModel, TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer)
    {
        var oldFieldSelector = FieldSelector;
        var fieldInformation = Util.GetFieldInformation(oldFieldSelector, Model);
        
        Expression<Func<TModel, TFieldTransformed>> transformedFieldSelector = model =>
            transformer(fieldInformation.FieldValue);
        
        var fieldValueTransformed = transformer(fieldInformation.FieldValue);
        return new ModelRule<TModel, TFieldTransformed>(
            Context,
            Model,
            fieldInformation.FieldFullPath,
            fieldValueTransformed,
            fieldInformation.IsFieldSelectedDifferentThanModel,
            transformedFieldSelector);
    }

    protected override ModelRule<TModel, TField> GetSelf()
    {
        return this;
    }

    protected override void HandleError(ValidationError error)
    {
        Context.AddError(error);
        
        if (Context is {ValidationMode: ValidationMode.StopOnFirstError, Errors: { }})
        {
            throw new ValidationException(Context.Errors!);
        }
    }

    public ModelRule<TModel, TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator)
    {
        Context.Clean(); // Ignore customizations for model validators
        var oldContext = Context;
        var childContext = Context.CloneForChildModelValidator(FieldFullPath);
        validator.Context = childContext;
        var childModel = (TChildModel)(object)FieldValue!;
        validator.Validate(childModel);
        var newErrors = validator.Context.Errors;
        validator.Context = oldContext;
        validator.Context.Errors = newErrors;
        return this;
    }
}