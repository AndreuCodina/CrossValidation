using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;

namespace CrossValidation;

/// <summary>
/// Settings to share between ValidationOperations and even Validations
/// </summary>
public class ValidationContext
{
    // public ValidationOperation ValidationTree { get; set; } = new ValidationOperation();
    public ValidationOperation MainValidationOperation { get; set; } = new();
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
    public bool HasAsyncValidations { get; set; } = false;
    public bool IsAsyncExecutionTriggered { get; set; } = false;

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

#pragma warning disable CS1998
    public async ValueTask ExecuteOperationsCollectedAsync<TField>(bool useAsync)
#pragma warning restore CS1998
    {
        throw new NotImplementedException();
        // var firstItemIndex = 0;
        //
        // while (ValidationOperationsCollected.Any())
        // {
        //     var operation = ValidationOperationsCollected[firstItemIndex];
        //     var isValid = await operation.ExecuteAsync(this, useAsync);
        //
        //     if (!isValid)
        //     {
        //         break;
        //     }
        //     
        //     ValidationOperationsCollected.RemoveAt(firstItemIndex);
        // }
        //
        // ValidationOperationsCollected.Clear();
        // CurrentOperation = new ValidationOperation();
    }
}