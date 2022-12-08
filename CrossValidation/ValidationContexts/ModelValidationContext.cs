namespace CrossValidation.ValidationContexts;

public class ModelValidationContext : ValidationContext
{
    // public bool IsChildContext { get; set; }
    public string? ParentPath { get; set; } = null;
    public ValidationMode ValidationMode { get; set; } = ValidationMode.StopOnFirstError;
    public bool IsChildContext { get; set; }
    

    public ModelValidationContext CloneForChildModelValidator(string parentPath)
    {
        var newContext = new ModelValidationContext();
        newContext.IsChildContext = true;
        newContext.Code = Code;
        newContext.Message = Message;
        newContext.ParentPath = parentPath;
        newContext.Errors = Errors;
        newContext.ValidationMode = ValidationMode;
        return newContext;
    }
}