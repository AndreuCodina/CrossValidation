using System.Linq.Expressions;
using System.Reflection;
using CrossValidation.Results;
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
    public Expression<Func<TModel, TField>> FieldSelector { get; set; }

    public string FieldFullPath { get; set; }
    
    public ModelRule(
        TModel model,
        Expression<Func<TModel, TField>> fieldSelector,
        ModelValidationContext context)
    {
        Context = context;
        Model = model;
        FieldSelector = fieldSelector;
        FieldFullPath = PathExpressionVisitor.Create(fieldSelector.Body).FieldFullPath;

        if (fieldSelector.Body is MemberExpression memberExpression)
        {
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            var currentModel = GetModelRelatedToField();
            FieldValue = (TField)propertyInfo.GetValue(currentModel)!;
        }
        else
        {
            FieldValue = (TField)((object)model!);
        }

        var parentPath = Context.ParentPath is not null ? Context.ParentPath + "." : "";
        context.FieldName = parentPath + FieldFullPath;
        context.FieldValue = FieldValue;
    }

    public object GetModelRelatedToField()
    {
        var nodeNames = FieldFullPath.Split('.')
            .SkipLast(1) // Remove the field selected
            .ToList();
        var hasSomeNode = !(nodeNames.Count == 1 && nodeNames[0] == "");

        if (!hasSomeNode)
        {
            return Model!;
        }

        object currentNodeModel = Model!;
        var propertiesInCurrentNodeModel = Model!.GetType().GetProperties();

        foreach (var node in nodeNames)
        {
            var propertyInfo = propertiesInCurrentNodeModel.First(x => x.Name == node);
            var childNodeModel = ConvertToChildField(propertyInfo, currentNodeModel);
            currentNodeModel = childNodeModel;
        }

        return currentNodeModel;
    }

    public object ConvertToChildField(PropertyInfo propertyInfo, object parent)
    {
        var source = propertyInfo.GetValue(parent, null)!;
        var destination = Activator.CreateInstance(propertyInfo.PropertyType)!;

        foreach (var prop in destination.GetType().GetProperties().ToList())
        {
            var value = source.GetType().GetProperty(prop.Name)!.GetValue(source, null);
            prop.SetValue(destination, value, null);
        }

        return destination;
    }

    public override ModelRule<TModel, TField> GetSelf()
    {
        return this;
    }

    public override void HandleError(ValidationError error)
    {
        Context.AddError(error);
        
        if (Context.ValidationMode == ValidationMode.StopOnFirstError
            && Context.Errors != null)
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