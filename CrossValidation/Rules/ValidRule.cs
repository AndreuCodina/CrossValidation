using System.Diagnostics;
using System.Linq.Expressions;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Utils;
using CrossValidation.ValidationContexts;

namespace CrossValidation.Rules;

public interface IValidRule<out TField> : IRule<TField>
{
    public TField GetFieldValue();
    public ValidationContext Context { get; set; }
    public string FieldFullPath { get; set; }
    void HandleError(IValidationError error);
    void TakeErrorCustomizations(IValidationError error, bool overrideContextCustomizations);
    
    public static IRule<TField> CreateFromField(
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null)
    {
        return new ValidRule<TField>(fieldValue, fieldFullPath, context, index, parentPath);
    }

    public static IRule<TField> CreateFromFieldSelector<TModel>(
        TModel model,
        Expression<Func<TModel, TField>> fieldSelector,
        ValidationContext? context = null)
    {
        var fieldInformation = FieldInformationExtractor<TField>
            .Extract(model, fieldSelector);
        return new ValidRule<TField>(fieldInformation.Value, fieldInformation.SelectionFullPath, context);
    }
}

file class ValidRule<TField> :
    Rule<TField>,
    IValidRule<TField>
{
    public TField FieldValue { get; set; }
    public ValidationContext Context { get; set; }
    public string FieldFullPath { get; set; }

    public ValidRule(
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null)
    {
        FieldValue = fieldValue;
        Context = context ?? new ValidationContext();
        Context.FieldValue = fieldValue;
        Context.FieldDisplayName = null;
        FieldFullPath = fieldFullPath ?? "";

        var indexRepresentation = index is not null
            ? $"[{index}]"
            : "";

        var parentPathValue = "";

        if (parentPath is not null)
        {
            parentPathValue = parentPath;
        }
        else if (Context.ParentPath is not null)
        {
            parentPathValue = Context.ParentPath;
        }

        if (parentPathValue is not "")
        {
            parentPathValue += ".";
        }

        Context.FieldName = parentPathValue + fieldFullPath + indexRepresentation;
    }

    public TField GetFieldValue()
    {
        return FieldValue;
    }
    
    public void TakeErrorCustomizations(IValidationError error, bool overrideContextCustomizations)
    {
        if (overrideContextCustomizations)
        {
            if (error.Code is not null)
            {
                Context.Code = error.Code;
            }

            if (error.Message is not null)
            {
                Context.Message = error.Message;
            }
        
            if (error.Details is not null)
            {
                Context.Details = error.Details;
            }
        }
    }

    public void HandleError(IValidationError error)
    {
        var errorToAdd = Context.Error ?? error;
        Context.AddError(errorToAdd);

        if (Context is {ValidationMode: ValidationMode.StopValidationOnFirstError})
        {
            throw new CrossValidationException(Context.ErrorsCollected!);
        }
    }

    public override TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        try
        {
            return fieldToInstance(FieldValue);
        }
        catch (CrossValidationException e)
        {
            var error = e.Errors[0];
            error.FieldName = null;
            error.FieldDisplayName = null;
            error.FieldValue = null;
            error.PlaceholderValues = null;
            TakeErrorCustomizations(error, overrideContextCustomizations: false);
            HandleError(error);
            throw new UnreachableException();
        }
    }
}