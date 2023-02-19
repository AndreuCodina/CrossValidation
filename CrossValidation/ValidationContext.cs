using System.Net;
using CrossValidation.Errors;

namespace CrossValidation;

public class ValidationContext
{
    public List<ICrossError>? ErrorsCollected { get; set; }
    public string? FieldName { get; set; }
    public string? ParentPath { get; set; }
    public ValidationMode ValidationMode { get; set; } = ValidationMode.StopValidationOnFirstError;
    public bool IsChildContext { get; set; }
    public ICrossError? Error { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public string? Details { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    public string? FieldDisplayName { get; set; }

    public ValidationContext CloneForChildModelValidator(string? parentPath)
    {
        var newContext = new ValidationContext
        {
            IsChildContext = true,
            ParentPath = parentPath,
            ErrorsCollected = ErrorsCollected,
            ValidationMode = ValidationMode
        };
        return newContext;
    }
}