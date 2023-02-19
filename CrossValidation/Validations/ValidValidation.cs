using System.Diagnostics;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Resources;

namespace CrossValidation.Validations;

public interface IValidValidation<out TField> :
    IValidation<TField>,
    IValidationCustomizations
{
    bool ExecuteNextValidator { get; set; }
    public TField GetFieldValue();
    public ValidationContext Context { get; set; }
    public string? FieldFullPath { get; set; }
    void HandleError(ICrossError error);
    void TakeErrorCustomizations(ICrossError error, bool overrideCustomizations);

    public static IValidation<TField> CreateFromField(
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null,
        IValidationCustomizations? customizations = null)
    {
        return new ValidValidation<TField>(fieldValue, fieldFullPath, context, index, parentPath, customizations);
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

        var fieldFullPath = fieldName.Substring(fieldName.IndexOf('.') + 1);
        return new ValidValidation<TField>(fieldValue, fieldFullPath, context);
    }

    void Clean();
}

file class ValidValidation<TField> :
    Validation<TField>,
    IValidValidation<TField>
{
    public TField FieldValue { get; set; }
    public ValidationContext Context { get; set; }
    public string? FieldFullPath { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public ICrossError? Error { get; set; }
    public string? FieldDisplayName { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    public bool ExecuteNextValidator { get; set; } = true;

    public ValidValidation(
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null,
        IValidationCustomizations? customizations = null)
    {
        FieldValue = fieldValue;
        Context = context ?? new ValidationContext();
        FieldDisplayName = null;
        FieldFullPath = fieldFullPath;
        
        if (customizations is not null)
        {
            Code = customizations.Code;
            Message = customizations.Message;
            Details = customizations.Details;
            Error = customizations.Error;
            FieldDisplayName = customizations.FieldDisplayName;
            HttpStatusCode = customizations.HttpStatusCode;
        }

        var indexRepresentation = Context.FieldName is not null && index is not null
            ? $"[{index}]"
            : null;

        string? parentPathValue = null;

        if (parentPath is not null)
        {
            parentPathValue = parentPath;
        }
        else if (Context.ParentPath is not null)
        {
            parentPathValue = Context.ParentPath;
        }

        if (parentPathValue is not null)
        {
            parentPathValue += ".";
        }
        
        Context.FieldName = parentPathValue + fieldFullPath + indexRepresentation;
        
        if (Context.FieldName is "")
        {
            Context.FieldName = null;
        }
    }

    public TField GetFieldValue()
    {
        return FieldValue;
    }

    public void TakeErrorCustomizations(ICrossError error, bool overrideCustomizations)
    {
        if (!overrideCustomizations)
        {
            return;
        }
        
        Code = error.Code ?? Code;
        Message = error.Message ?? Message;
        Details = error.Details ?? Details;
        HttpStatusCode = error.HttpStatusCode ?? HttpStatusCode;
        FieldDisplayName = error.FieldDisplayName ?? FieldDisplayName;
    }

    public void HandleError(ICrossError error)
    {
        var errorToAdd = Error ?? error;
        AddError(errorToAdd);

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
            e.Error.FieldValue = null;
            e.Error.PlaceholderValues = null;
            TakeErrorCustomizations(e.Error, overrideCustomizations: false);
            HandleError(e.Error);
            throw new UnreachableException();
        }
    }

    public void Clean()
    {
        Code = null;
        Message = null;
        Details = null;
        Error = null;
        HttpStatusCode = null;
        FieldDisplayName = null;
        ExecuteNextValidator = true;
    }

    public void AddError(ICrossError error)
    {
        AddCustomizationsToError(error);
        Context.ErrorsCollected ??= new List<ICrossError>();
        error.AddPlaceholderValues();
        Context.ErrorsCollected.Add(error);
    }

    private void AddCustomizationsToError(ICrossError error)
    {
        error.FieldName = Context.FieldName;
        error.FieldDisplayName = GetFieldDisplayNameToFill(error);
        error.FieldValue = FieldValue;
        error.Code = Code ?? error.Code;
        error.Message = GetMessageToFill(error);
        error.Details = Details ?? error.Details;
        error.HttpStatusCode = HttpStatusCode ?? error.HttpStatusCode;
    }

    private string? GetMessageToFill(ICrossError error)
    {
        if (Message is not null)
        {
            return Message;
        }

        if (Code is not null)
        {
            return ErrorResource.ResourceManager.GetString(Code);
        }

        if (error.Message is not null)
        {
            return error.Message;
        }

        if (error.Code is not null)
        {
            return ErrorResource.ResourceManager.GetString(error.Code);
        }

        return null;
    }

    private string GetFieldDisplayNameToFill(ICrossError error)
    {
        if (FieldDisplayName is not null)
        {
            return FieldDisplayName;
        }
        
        if (error.FieldDisplayName is not null)
        {
            return error.FieldDisplayName;
        }

        return error.FieldName!;
    }
}