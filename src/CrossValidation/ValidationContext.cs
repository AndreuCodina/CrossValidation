using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;

namespace CrossValidation;

/// <summary>
/// Settings to share between validations
/// </summary>
public class ValidationContext
{
    public IValidationOperation? ValidationTree { get; set; }
    public List<ICrossError> ErrorsCollected { get; set; } = new();
    public ValidationMode ValidationMode { get; set; } = ValidationMode.StopOnFirstError;
    public bool IsChildContext { get; set; }
    public ICrossError? Error { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public string? Details { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    public string? FieldDisplayName { get; set; }
    public ICrossErrorToException? CrossErrorToException { get; set; }

    public ValidationContext CloneForChildModelValidator()
    {
        return new ValidationContext
        {
            IsChildContext = true,
            ErrorsCollected = ErrorsCollected,
            ValidationMode = ValidationMode,
            CrossErrorToException = CrossErrorToException
        };
    }
}