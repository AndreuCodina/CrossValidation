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

    private ModelRule(
        ModelValidationContext context,
        TModel model,
        string fieldFullPath,
        TField fieldValue,
        bool isMember)
    {
        Context = context;
        Model = model;
        FieldFullPath = fieldFullPath;
        var fieldValueToAssign = isMember ? fieldValue : default;
        FieldValue = fieldValueToAssign;
        Context.FieldValue = fieldValueToAssign;
        var parentPath = context.ParentPath is not null ? context.ParentPath + "." : "";
        Context.FieldName = parentPath + fieldFullPath;
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
            fieldInformation.IsFieldSelectedDifferentThanModel);
    }
    
    public ModelRule<TModel, TTransformed> Transform<TTransformed>(
        Expression<Func<TModel, TField>> oldFieldSelector,
        Func<TField, TTransformed> transformer)
    {
        var fieldInformation = Util.GetFieldInformation(oldFieldSelector, Model);
        var fieldValueTransformed = transformer(fieldInformation.FieldValue);
        return new ModelRule<TModel, TTransformed>(
            Context,
            Model,
            fieldInformation.FieldFullPath,
            fieldValueTransformed,
            fieldInformation.IsFieldSelectedDifferentThanModel);
    }

    public override ModelRule<TModel, TField> GetSelf()
    {
        return this;
    }

    public override void HandleError(ValidationError error)
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