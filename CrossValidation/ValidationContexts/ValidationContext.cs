using CrossValidation.Results;

namespace CrossValidation.ValidationContexts;

public abstract class ValidationContext
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public List<ValidationError>? Errors { get; set; }
    public string FieldName { get; set; } = "";
    public object? FieldValue { get; set; }
    public ValidationError? Error { get; set; }
    public bool ExecuteNextValidator { get; set; } = true;

    public ValidationContext()
    {
    }

    public void AddError(ValidationError error)
    {
        Errors ??= new List<ValidationError>();
        SetError(error);
        Error!.AddPlaceHolderValues();
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
    
    public void SetError(ValidationError error)
    {
        Error = CustomizationsToError(error);
    }

    private ValidationError CustomizationsToError(ValidationError error)
    {
        ValidationError errorToCustomize;
        
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
    
    private void CombineErrors(ValidationError originalError, ValidationError errorToCombine)
    {
        originalError.Code ??= errorToCombine.Code;
        originalError.Message ??= errorToCombine.Message;
    }
}