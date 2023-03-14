using System.Diagnostics;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Resources;

namespace CrossValidation.Validations;

public interface IValidValidation<out TField> : IValidation<TField>
{
    // public List<ValidationOperation> ValidationOperationsCollected { get; set; }
    // public ValidationOperation ValidationOperation { get; set; }
    public TField GetFieldValue();
    public ValidationContext Context { get; set; }
    public string? FieldFullPath { get; set; }
    public Type CrossErrorToException { get; set; }

    public static IValidation<TField> CreateFromField(
        Func<TField> getFieldValue,
        Type crossErrorToException,
        bool generalizeError = true,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null)
    {
        return new ValidValidation<TField>(
            getFieldValue: getFieldValue,
            crossErrorToException: crossErrorToException,
            generalizeError: generalizeError,
            fieldFullPath: fieldFullPath,
            context: context,
            index: index,
            parentPath: parentPath,
            error: error,
            message: message,
            code: code,
            details: details,
            httpStatusCode: httpStatusCode,
            fieldDisplayName: fieldDisplayName);
    }

    public static IValidation<TField> CreateFromFieldName(
        Func<TField> fieldValue,
        Type crossErrorToException,
        string fieldName,
        bool allowFieldNameWithoutModel,
        ValidationContext? context = null,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null)
    {
        if (!allowFieldNameWithoutModel)
        {
            if (!fieldName.Contains("."))
            {
                throw new ArgumentException("Use Field without a model is not allowed");
            }
        }

        var fieldFullPath = allowFieldNameWithoutModel
            ? fieldName
            : fieldName.Substring(fieldName.IndexOf('.') + 1);
        return new ValidValidation<TField>(
            getFieldValue: fieldValue,
            crossErrorToException: crossErrorToException,
            generalizeError: false,
            fieldFullPath: fieldFullPath,
            context: context,
            error: error,
            message: message,
            code: code,
            details: details,
            httpStatusCode: httpStatusCode,
            fieldDisplayName: fieldDisplayName);
    }
}

// public interface IValidationScope
// {
//     
// }

file class ValidValidation<TField> :
    Validation<TField>,
    IValidValidation<TField>
{
    // public TField FieldValue { get; set; }
    public Func<TField> GetFieldValueTransformed { get; set; }
    // public List<ValidationOperation> ValidationOperationsCollected { get; set; } = new();
    // public ValidationOperation ValidationOperation { get; set; } = new();
    public ValidationContext Context { get; set; }
    public string? FieldFullPath { get; set; }
    public Type CrossErrorToException { get; set; }

    public ValidValidation(
        Func<TField> getFieldValue,
        Type crossErrorToException,
        bool generalizeError,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null)
    {
        if (context != null)
        {
            Context = context;
        }
        else
        {
            Context = new ValidationContext();
        }
        
        Context.ValidationOperation = new ValidationOperation<TField>();
        GetFieldValueTransformed = getFieldValue;
        Context.ValidationOperation!.GetFieldValue = () => getFieldValue;
        CrossErrorToException = crossErrorToException;
        Context.ValidationOperation.FieldDisplayName = null;
        FieldFullPath = fieldFullPath;
        Context.GeneralizeError = Context.IsChildContext && Context.GeneralizeError
            ? true
            : generalizeError;

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

        Context.Error = error;
        Context.Message = message;
        Context.Code = code;
        Context.Details = details;
        Context.HttpStatusCode = httpStatusCode;
        Context.FieldDisplayName = fieldDisplayName;
    }

    public TField GetFieldValue()
    {
        return GetFieldValueTransformed();
    }

    public override TField Instance()
    {
        return GetFieldValueTransformed();
    }

    public override TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        try
        {
            return fieldToInstance(GetFieldValueTransformed());
        }
        catch (CrossException e)
        {
            e.Error.FieldName = null;
            e.Error.PlaceholderValues = null;
            e.Error.CrossErrorToException = CrossErrorToException;
            Context.ValidationOperation!.TakeCustomizationsFromInstanceError(e.Error, Context);
            Context.ValidationOperation.HandleError(e.Error, Context);
            throw new UnreachableException();
        }
    }
}