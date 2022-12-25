using System.Linq.Expressions;
using CrossValidation.Results;
using CrossValidation.Utils;
using CrossValidation.ValidationContexts;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public class ModelRule<TModel, TField>
    : Rule<
        ModelRule<TModel, TField>,
        TField,
        ModelValidationContext>
{
    public TModel Model { get; set; }
    public string FieldFullPath { get; set; }

    private ModelRule(
        TField? fieldValue,
        ModelValidationContext context,
        TModel model,
        string fieldFullPath) :
        base(fieldValue, context)
    {
        Model = model;
        FieldFullPath = fieldFullPath;
    }
    
    // CreateFromField
    public ModelRule(
        ModelValidationContext context,
        TModel model,
        string fieldFullPath,
        TField? fieldValue,
        int? index = null,
        string? parentPath = null) :
        this(fieldValue, context, model, fieldFullPath)
    {
        Context.FieldValue = fieldValue;
        var indexRepresentation = index is not null
            ? $"[{index}]"
            : "";

        var parentPathValue = "";
        
        if (parentPath is not null)
        {
            parentPathValue = parentPath;
        }
        else if (context.ParentPath is not null)
        {
            parentPathValue = context.ParentPath;
        }
        
        if (parentPathValue is not "")
        {
            parentPathValue += ".";
        }

        Context.FieldName = parentPathValue + fieldFullPath + indexRepresentation;
        Context.FieldDisplayName = null;
    }

    public static ModelRule<TModel, TField> CreateFromFieldSelector(
        TModel model,
        ModelValidationContext context,
        Expression<Func<TModel, TField?>> fieldSelector)
    {
        var fieldInformation = Util.GetFieldInformation(fieldSelector!, model);
        return new ModelRule<TModel, TField>(
            context,
            model,
            fieldInformation.SelectionFullPath,
            fieldInformation.Value);
    }

    public ModelRule<TModel, TFieldTransformed?> Transform<TFieldTransformed>(
        Func<TField?, TFieldTransformed?> transformer)
    {
        var fieldValueTransformed = transformer(FieldValue);
        return new ModelRule<TModel, TFieldTransformed?>(
            Context,
            Model,
            Context.FieldName,
            fieldValueTransformed);
    }

    protected override ModelRule<TModel, TField> GetSelf()
    {
        return this;
    }

    protected override void HandleError(CrossValidationError error)
    {
        Context.AddError(error);
        
        if (Context is {ValidationMode: ValidationMode.StopOnFirstError, Errors: { }})
        {
            throw new CrossValidationException(Context.Errors!);
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
    
    public ModelRule<TModel, TField> When(Func<TModel, bool> condition)
    {
        Context.ExecuteNextValidator = condition(Model);
        return this;
    }
    
    public ModelRule<TModel, TField> Must(Func<TModel, bool> condition)
    {
        SetValidator(new PredicateValidator(condition(Model)));
        return this;
    }
}