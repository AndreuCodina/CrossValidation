using System.Diagnostics;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.ValidationContexts;

namespace CrossValidation.Rules;

public interface IValidRule<out TField> : IRule<TField>
{
    public TField GetFieldValue();
    public ValidationContext Context { get; set; }
    public string FieldFullPath { get; set; }
    void HandleError(ICrossError error);
    void TakeErrorCustomizations(ICrossError error, bool overrideContextCustomizations);
    
    public static IRule<TField> CreateFromField(
        Dsl dsl,
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null)
    {
        return new ValidRule<TField>(dsl, fieldValue, fieldFullPath, context, index, parentPath);
    }

    public static IRule<TField> CreateFromFieldName(
        Dsl dsl,
        TField fieldValue,
        string fieldName,
        ValidationContext? context = null)
    {
        if (!fieldName.Contains("."))
        {
            throw new ArgumentException("Use Field without a model is not allowed");
        }

        var selectionFullPath = fieldName.Substring(fieldName.IndexOf('.') + 1);
        return new ValidRule<TField>(dsl, fieldValue, selectionFullPath, context);
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
        Dsl dsl,
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null)
    {
        FieldValue = fieldValue;
        Context = context ?? new ValidationContext();
        Context.Dsl = dsl;
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

    private EnsureError ToEnsureError(ICrossError error)
    {
        return new EnsureError
        {
            Details = error.Details,
            HttpStatusCode = error.HttpStatusCode
        };
    }

    public void HandleError(ICrossError error)
    {
        var errorToAdd = Context.Error ?? error;
        Context.AddError(errorToAdd);
        
        if (Context.Dsl == Dsl.Ensure)
        {
            throw new EnsureException(ToEnsureError(Context.ErrorsCollected![0]));
        }

        if (Context is {ValidationMode: ValidationMode.StopValidationOnFirstError})
        {
            if (Context.Dsl == Dsl.Validate)
            {
                if (Context.ErrorsCollected!.Count == 1)
                {
                    throw Context.ErrorsCollected[0].ToException();
                }
            }
            else
            {
                throw new UnreachableException();
            }
        }
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