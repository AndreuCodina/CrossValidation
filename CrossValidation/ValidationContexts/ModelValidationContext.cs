namespace CrossValidation.ValidationContexts;

public class ModelValidationContext : ValidationContext
{
    public string? ParentPath { get; set; }
    public ValidationMode ValidationMode { get; set; } = ValidationMode.StopOnFirstError;
    public bool IsChildContext { get; set; }

    public ModelValidationContext CloneForChildModelValidator(string parentPath)
    {
        var newContext = new ModelValidationContext
        {
            IsChildContext = true,
            ParentPath = parentPath,
            Errors = Errors,
            ValidationMode = ValidationMode
        };
        return newContext;
    }
}