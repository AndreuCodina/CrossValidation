using System.Diagnostics;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.ValidationContexts;

namespace CrossValidation.Validations;

public interface IValidValidation<out TField> : IValidation<TField>
{
    public TField GetFieldValue();
    public ValidationContext Context { get; set; }
    public string FieldFullPath { get; set; }
    void HandleError(ICrossError error);
    void TakeErrorCustomizations(ICrossError error, bool overrideContextCustomizations);
    
    public static IValidation<TField> CreateFromField(
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null)
    {
        return new ValidValidation<TField>(fieldValue, fieldFullPath, context, index, parentPath);
    }

    public static IValidation<TField> CreateFromFieldName(
        TField fieldValue,
        string fieldName,
        ValidationContext? context = null)
    {
        if (!fieldName.Contains("."))
        {
            throw new ArgumentException("Use Field without a model is not allowed");
        }

        var selectionFullPath = fieldName.Substring(fieldName.IndexOf('.') + 1);
        return new ValidValidation<TField>(fieldValue, selectionFullPath, context);
    }
}

file class ValidValidation<TField> :
    Validation<TField>,
    IValidValidation<TField>
{
    public TField FieldValue { get; set; }
    public ValidationContext Context { get; set; }
    public string FieldFullPath { get; set; }

    public ValidValidation(
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
    
    public void TakeErrorCustomizations(ICrossError error, bool overrideContextCustomizations)
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
            
            if (error.HttpStatusCode is not null)
            {
                Context.HttpStatusCode = error.HttpStatusCode;
            }
        }
    }

    public void HandleError(ICrossError error)
    {
        var errorToAdd = Context.Error ?? error;
        Context.AddError(errorToAdd);

        if (Context is {ValidationMode: ValidationMode.StopValidationOnFirstError})
        {
            if (Context.ErrorsCollected!.Count == 1)
            {
                throw Context.ErrorsCollected[0].ToException();
            }
        }
    }
    
    public override TField Instance()
    {
        return FieldValue;
    }

    public override TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        try
        {
            return fieldToInstance(FieldValue);
        }
        catch (CrossException e)
        {
            e.Error.FieldName = null;
            e.Error.FieldDisplayName = null;
            e.Error.FieldValue = null;
            e.Error.HttpStatusCode = null;
            e.Error.PlaceholderValues = null;
            TakeErrorCustomizations(e.Error, overrideContextCustomizations: false);
            HandleError(e.Error);
            throw new UnreachableException();
        }
    }
}