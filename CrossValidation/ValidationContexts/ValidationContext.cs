using CrossValidation.Results;

namespace CrossValidation.ValidationContexts;

public abstract class ValidationContext
{
    public string? Code { get; set; } = null;
    public string? Message { get; set; } = null;
    public List<ValidationError>? Errors { get; set; } = null;
    public string FieldName { get; set; } = "";
    public object? FieldValue { get; set; } = null;

    public ValidationContext()
    {
        // Errors = new List<ValidationError>();
    }

    public void AddError(ValidationError error)
    {
        Errors ??= new List<ValidationError>();
        var errorToAdd = new ValidationError(
            FieldName: error.FieldName,
            FieldValue: error.FieldValue,
            Code: Code ?? error.Code,
            Message: Message ?? error.Message,
            Detail: error.Detail,
            Parameters: error.Parameters);
        Errors.Add(errorToAdd); // If the validationPath exists, add another error -> ToDictionary -> Dictionary<FieldName, List<IError/ErrorCode>>
    }

    public void Clean()
    {
        Code = null;
        Message = null;
    }

    public void SetCode(string code)
    {
        Code ??= code;
    }
    
    public void SetMessage(string message)
    {
        Message ??= message;
    }
}