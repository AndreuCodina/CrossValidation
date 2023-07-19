using System.Net;
using CrossValidation.Exceptions;

namespace CrossValidation;

/// <summary>
/// Settings to share between validations
/// </summary>
public class ValidationContext
{
    public IValidationOperation? ValidationTree { get; set; }
    public List<BusinessException> ExceptionsCollected { get; set; } = new();
    public ValidationMode ValidationMode { get; set; } = ValidationMode.StopOnFirstError;
    public bool IsChildContext { get; set; }
    public BusinessException? Error { get; set; }
    public string Message { get; set; } = "";
    public string? Code { get; set; }
    public string? Details { get; set; }
    public HttpStatusCode? StatusCode { get; set; }
    public string? FieldDisplayName { get; set; }

    public ValidationContext CloneForChildModelValidator()
    {
        return new ValidationContext
        {
            IsChildContext = true,
            ExceptionsCollected = ExceptionsCollected,
            ValidationMode = ValidationMode
        };
    }
}