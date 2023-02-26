using System.Diagnostics;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Resources;

namespace CrossValidation.Validations;

public interface IValidValidation<out TField> : IValidation<TField>
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public ICrossError? Error { get; set; }
    public string? FieldDisplayName { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    bool ExecuteNextValidator { get; set; }
    public TField GetFieldValue();
    public ValidationContext Context { get; set; }
    public string? FieldFullPath { get; set; }
    public Type CrossErrorToException { get; set; }
    void HandleError(ICrossError error);
    void TakeErrorCustomizations(ICrossError error, bool overrideCustomizations);

    public static IValidation<TField> CreateFromField(
        TField fieldValue,
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
            fieldValue: fieldValue,
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
        TField fieldValue,
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
            fieldValue: fieldValue,
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

    void Clean();
}

file class ValidValidation<TField> :
    Validation<TField>,
    IValidValidation<TField>
{
    private string? _code;
    private string? _message;
    private string? _details;
    private ICrossError? _error;
    private string? _fieldDisplayName;
    private HttpStatusCode? _httpStatusCode;

    public TField FieldValue { get; set; }
    public ValidationContext Context { get; set; }
    public string? FieldFullPath { get; set; }
    public Type CrossErrorToException { get; set; }

    public string? Code
    {
        get
        {
            if (Context.Code is not null)
            {
                return Context.Code;
            }

            return _code;
        }
        set
        {
            if (Context.Code is not null)
            {
                return;
            }

            _code = value;
        }
    }

    public string? Message
    {
        get
        {
            if (Context.Message is not null)
            {
                return Context.Message;
            }

            return _message;
        }
        set
        {
            if (Context.Message is not null)
            {
                return;
            }

            _message = value;
        }
    }

    public string? Details
    {
        get
        {
            if (Context.Details is not null)
            {
                return Context.Details;
            }

            return _details;
        }
        set
        {
            if (Context.Details is not null)
            {
                return;
            }

            _details = value;
        }
    }

    public ICrossError? Error
    {
        get
        {
            if (Context.Error is not null)
            {
                return Context.Error;
            }

            return _error;
        }
        set
        {
            if (Context.Error is not null)
            {
                return;
            }

            _error = value;
        }
    }

    public string? FieldDisplayName
    {
        get
        {
            if (Context.FieldDisplayName is not null)
            {
                return Context.FieldDisplayName;
            }

            return _fieldDisplayName;
        }
        set
        {
            if (Context.FieldDisplayName is not null)
            {
                return;
            }

            _fieldDisplayName = value;
        }
    }

    public HttpStatusCode? HttpStatusCode
    {
        get
        {
            if (Context.HttpStatusCode is not null)
            {
                return Context.HttpStatusCode;
            }

            return _httpStatusCode;
        }
        set
        {
            if (Context.HttpStatusCode is not null)
            {
                return;
            }

            _httpStatusCode = value;
        }
    }

    public bool ExecuteNextValidator { get; set; } = true;

    public ValidValidation(
        TField fieldValue,
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
        FieldValue = fieldValue;
        CrossErrorToException = crossErrorToException;
        Context = context ?? new ValidationContext();
        FieldDisplayName = null;
        FieldFullPath = fieldFullPath;
        Context.GeneralizeError = generalizeError;

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
            e.Error.CrossErrorToException = CrossErrorToException;
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

    private void AddError(ICrossError error)
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
        
        if (Code is not null)
        {
            error.Code = Code;
        }
        else if (Context.GeneralizeError)
        {
            error.Code = nameof(ErrorResource.General);
        }
        else
        {
            error.Code = error.Code;
        }
        
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
            // if (Context.GeneralizeError)
            // {
            //     return CrossValidationOptions.GetMessageFromCode(nameof(ErrorResource.General));
            // }
            
            return CrossValidationOptions.GetMessageFromCode(Code);
        }

        if (error.Message is not null)
        {
            if (Context.GeneralizeError)
            {
                return CrossValidationOptions.GetMessageFromCode(nameof(ErrorResource.General));
            }
            
            return error.Message;
        }

        if (error.Code is not null)
        {
            if (Context.GeneralizeError)
            {
                return CrossValidationOptions.GetMessageFromCode(nameof(ErrorResource.General));
            }
            
            return CrossValidationOptions.GetMessageFromCode(error.Code);
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