using CrossValidation.Results;

namespace CrossValidation.ValidationContexts;

public class ValidationContext
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public List<CrossValidationError>? Errors { get; set; }
    public string FieldName { get; set; } = "";
    public string? FieldDisplayName { get; set; }
    public object? FieldValue { get; set; }
    public CrossValidationError? Error { get; set; }
    public bool ExecuteNextValidator { get; set; } = true;
    public string? ParentPath { get; set; }
    public ValidationMode ValidationMode { get; set; } = ValidationMode.StopValidationOnFirstError;
    public bool IsChildContext { get; set; }

    public ValidationContext CloneForChildModelValidator(string parentPath)
    {
        var newContext = new ValidationContext
        {
            IsChildContext = true,
            ParentPath = parentPath,
            Errors = Errors,
            ValidationMode = ValidationMode
        };
        return newContext;
    }

    public void AddError(CrossValidationError error)
    {
        Errors ??= new List<CrossValidationError>();
        SetError(error);
        Error!.AddPlaceholderValues();
        Errors.Add(Error);
    }

    public void Clean()
    {
        Code = null;
        Message = null;
        Error = null;
        ExecuteNextValidator = true;
    }

    public void SetCode(string code)
    {
        Code ??= code;
    }
    
    public void SetMessage(string message)
    {
        Message ??= message;
    }
    
    public void SetError(CrossValidationError error)
    {
        Error = CustomizationsToError(error);
    }

    public void SetFieldDisplayName(string fieldDisplayName)
    {
        FieldDisplayName = fieldDisplayName;
    }

    private CrossValidationError CustomizationsToError(CrossValidationError error)
    {
        CrossValidationError errorToCustomize;
        
        if (Error is not null)
        {
            errorToCustomize = Error;
            CombineErrors(errorToCustomize, error);
        }
        else
        {
            errorToCustomize = error;
        }
        
        errorToCustomize.Code = Code ?? error.Code;
        errorToCustomize.Message = Message ?? error.Message;
        return errorToCustomize;
    }
    
    private void CombineErrors(CrossValidationError originalError, CrossValidationError errorToCombine)
    {
        originalError.Code ??= errorToCombine.Code;
        originalError.Message ??= errorToCombine.Message;
        originalError.FieldName ??= errorToCombine.FieldName;
        originalError.FieldDisplayName ??= errorToCombine.FieldDisplayName;
    }
}