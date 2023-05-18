using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;

namespace CrossValidation;

/// <summary>
/// Settings to share between ValidationOperations and even Validations
/// </summary>
public class ValidationContext
{
    public IValidationOperation? ValidationTree { get; set; }
    public List<ICrossError>? ErrorsCollected { get; set; }
    public string? FieldName { get; set; }
    public string? ParentPath { get; set; }
    public ValidationMode ValidationMode { get; set; } = ValidationMode.StopOnFirstError;
    public bool IsChildContext { get; set; }
    public ICrossError? Error { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public string? Details { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    public string? FieldDisplayName { get; set; }
    public ICrossErrorToException? CrossErrorToException { get; set; }
    public bool GeneralizeError { get; set; } = false;

    public ValidationContext CloneForChildModelValidator(string? parentPath)
    {
        var newContext = new ValidationContext
        {
            IsChildContext = true,
            ParentPath = parentPath,
            ErrorsCollected = ErrorsCollected,
            ValidationMode = ValidationMode,
            CrossErrorToException = CrossErrorToException,
            GeneralizeError = GeneralizeError
        };
        return newContext;
    }
}