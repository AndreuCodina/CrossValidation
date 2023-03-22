using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;

namespace CrossValidation;

/// <summary>
/// Settings to share between ValidationOperations and even Validations
/// </summary>
public class ValidationContext
{
    public List<IValidationOperation> ValidationOperationsCollected { get; set; } = new();
    public IValidationOperation? ValidationOperation { get; set; }
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

    public async ValueTask ExecuteOperationsCollectedAsync<TField>(bool useAsync)
    {
        var firstItemIndex = 0;
        
        while (ValidationOperationsCollected.Any())
        {
            var operation = ValidationOperationsCollected[firstItemIndex];
            var isValid = await operation.ExecuteAsync(this, useAsync);

            if (!isValid)
            {
                break;
            }
            
            ValidationOperationsCollected.RemoveAt(firstItemIndex);
        }

        ValidationOperationsCollected.Clear();
        ValidationOperation = new ValidationOperation<TField>();
    }
}