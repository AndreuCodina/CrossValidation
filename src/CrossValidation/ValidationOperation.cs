using System.Net;
using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.Validators;

namespace CrossValidation;

/// <summary>
/// Manages a validation operation
///
/// In a validation such as:
/// 
/// Validate.That(request.Age)
///     .WithMessage("The age is required").NotNull() // Validation operation
///     .WithMessage("The age must be greater than or equal to 18").GreaterThanOrEqual(18) // Validation operation
///     .Transform(age => age + 1) // Validation operation
///     .Must(() => true) // Validation operation
/// </summary>
public interface IValidationOperation
{
    Func<object>? GetNonGenericFieldValue { get; set; }
    Func<IValidator<ICrossError>>? Validator { get; set; }
    Func<Task<IValidator<ICrossError>>>? AsyncValidator { get; set; }
    Func<bool>? ValidationScope { get; set; }
    string? Code { get; set; }
    string? Message { get; set; }
    string? Details { get; set; }
    ICrossError? Error { get; set; }
    string? FieldDisplayName { get; set; }
    HttpStatusCode? HttpStatusCode { get; set; }
    // Type CrossErrorToException { get; set; }
    Func<bool>? Condition { get; set; }
    Func<Task<bool>>? AsyncCondition { get; set; }
    IValidationOperation? NextValidation { get; set; }
    bool HasFailed { get; set; }
    bool HasBeenExecuted { get; set; }
    // bool HasAsyncNextValidationsPendingToExecute { get; set; } = false;
    bool HasPendingAsyncValidation { get; set; }
    bool IsScopeCreator { get; set; }
    List<IValidationOperation>? DependentValidations { get; set; }
    int? Index { get; set; }
    string? ParentPath { get; set; } 
    public bool IsInsideScope { get; set; }
    public IValidationOperation? ScopeCreatorValidation { get; set; }
    ValueTask TraverseAsync(ValidationContext context);
    ValueTask<bool> ExecuteAsync(ValidationContext context, bool useAsync);
    void HandleError(ICrossError error, ValidationContext context);
    void TakeCustomizationsFromInstanceError(ICrossError error, ValidationContext context);
    void TakeCustomizationsFromError(ICrossError error);
    void MarkAsPendingAsyncValidation();
    void MarkAsFailed();
}

internal class ValidationOperation
{
    public Func<object>? GetNonGenericFieldValue { get; set; }
    public Func<IValidator<ICrossError>>? Validator { get; set; }
    public Func<Task<IValidator<ICrossError>>>? AsyncValidator { get; set; }
    public Func<bool>? ValidationScope { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public ICrossError? Error { get; set; }
    public string? FieldDisplayName { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    // public Type CrossErrorToException { get; set; }
    public Func<bool>? Condition { get; set; }
    public Func<Task<bool>>? AsyncCondition { get; set; }
    public IValidationOperation? NextValidation { get; set; }
    public bool HasFailed { get; set; }
    public bool HasBeenExecuted { get; set; }
    // public bool HasAsyncNextValidationsPendingToExecute { get; set; } = false;
    public bool HasPendingAsyncValidation { get; set; }
    public bool IsScopeCreator { get; set; }
    public List<IValidationOperation>? DependentValidations { get; set; }
    public int? Index { get; set; }
    public string? ParentPath { get; set; }
    public bool IsInsideScope { get; set; }
    public IValidationOperation? ScopeCreatorValidation { get; set; }

    // public void SetValidationScope(Action setScope);
    // {
    //     ValidationScope = validationScope;
    // }
    
    public async ValueTask TraverseAsync(ValidationContext context)
    {
        if (HasFailed
            && (!IsScopeCreator
                || (IsScopeCreator && context.ValidationMode is not ValidationMode.AccumulateFirstErrorAndAllFirstErrorsCollectionIteration)))
        {
            return;
        }
        
        if (!HasBeenExecuted)
        {
            await ExecuteAsync(context, useAsync: true);
        }

        while (IsScopeCreator && HasPendingAsyncValidation)
        {
            HasPendingAsyncValidation = false;

            foreach (var dependentValidation in DependentValidations!)
            {
                await dependentValidation.TraverseAsync(context);
            }
        }
        
        if (NextValidation is not null)
        {
            await NextValidation.TraverseAsync(context);
        }
    }
    
    public async ValueTask<bool> ExecuteAsync(ValidationContext context, bool useAsync)
    {
        if (Condition is not null)
        {
            if (!Condition())
            {
                return true;
            }
        }
        else if (AsyncCondition is not null)
        {
            if (!useAsync)
            {
                throw new InvalidOperationException("An asynchronous condition cannot be used in synchronous mode");
            }
            
            if (!await AsyncCondition())
            {
                return true;
            }
        }
        
        if (Validator is not null)
        {
            var error = Validator!().GetError();

            if (error is null)
            {
                return true;
            }

            // TODO
            // error.CrossErrorToException = CrossErrorToException;
            HandleError(error, context);
            // validValidation.Clean();
            return false;
        }
        else if (AsyncValidator is not null)
        {
            if (!useAsync)
            {
                throw new InvalidOperationException("An asynchronous validator cannot be used in synchronous mode");
            }
            
            var error = (await AsyncValidator()).GetError();

            if (error is null)
            {
                return true;
            }

            // TODO
            // error.CrossErrorToException = CrossErrorToException;
            HandleError(error, context);
            // validValidation.Clean();
            return false;
        }
        else if (ValidationScope is not null)
        {
            return ValidationScope();
        }

        HasBeenExecuted = true;
        return true;
    }

    public void HandleError(ICrossError error, ValidationContext context)
    {
        var errorToAdd = Error ?? error;
        AddError(errorToAdd, context);

        if (context is {ValidationMode: ValidationMode.StopOnFirstError})
        {
            if (context.ErrorsCollected!.Count == 1)
            {
                throw context.ErrorsCollected[0].ToException();
            }
        }
    }
    
    public void TakeCustomizationsFromInstanceError(ICrossError error, ValidationContext context)
    {
        if (!context.GeneralizeError)
        {
            return;
        }

        var codeToAdd = Code ?? error.Code;
        var isInstanceCallerWithCodeAndWithoutMessage = Code is not null && Message is null;
        
        if (isInstanceCallerWithCodeAndWithoutMessage && codeToAdd is not null)
        {
            Message = CrossValidationOptions.GetMessageFromCode(codeToAdd);
        }
        else
        {
            Message ??= error.Message;
        }

        Code = codeToAdd;
    }
    
    public void TakeCustomizationsFromError(ICrossError error)
    {
        Code = error.Code ?? Code;
        Message = error.Message ?? Message;
        Details = error.Details ?? Details;
        HttpStatusCode = error.HttpStatusCode ?? HttpStatusCode;
        FieldDisplayName = error.FieldDisplayName ?? FieldDisplayName;
    }
    
    public void MarkAsPendingAsyncValidation()
    {
        HasPendingAsyncValidation = true;
        
        if (IsInsideScope)
        {
            ScopeCreatorValidation!.MarkAsPendingAsyncValidation();
        }
    }

    public void MarkAsFailed()
    {
        HasFailed = true;
        
        if (IsInsideScope)
        {
            ScopeCreatorValidation!.MarkAsFailed();
        }
    }
    
    private void AddError(ICrossError error, ValidationContext context)
    {
        AddCustomizationsToError(error, context);
        context.ErrorsCollected ??= new List<ICrossError>();
        error.AddPlaceholderValues();
        context.ErrorsCollected.Add(error);
    }

    private void AddCustomizationsToError(ICrossError error, ValidationContext context)
    {
        error.FieldName = context.FieldName; // TODO: Save it in ValidationOperation
        error.FieldDisplayName = GetFieldDisplayNameToFill(error);
        error.Code = GetCodeToFill(error, context);
        error.Message = GetMessageToFill(error, context);
        error.Details = Details ?? error.Details;
        error.HttpStatusCode = HttpStatusCode ?? error.HttpStatusCode;
    }

    private string? GetCodeToFill(ICrossError error, ValidationContext context)
    {
        if (Code is not null)
        {
            return Code;
        }
        
        if (context.GeneralizeError) // TODO: Save it in ValidationOperation
        {
            return nameof(ErrorResource.General);
        }

        return error.Code;
    }

    private string? GetMessageToFill(ICrossError error, ValidationContext context)
    {
        if (Message is not null)
        {
            return Message;
        }

        if (Code is not null)
        {
            return CrossValidationOptions.GetMessageFromCode(Code);
        }
        
        if (context.GeneralizeError) // TODO: Save it in ValidationOperation
        {
            return CrossValidationOptions.GetMessageFromCode(nameof(ErrorResource.General));
        }

        if (error.Message is not null)
        {
            return error.Message;
        }

        if (error.Code is not null)
        {
            return CrossValidationOptions.GetMessageFromCode(error.Code);
        }

        return null;
    }

    private string? GetFieldDisplayNameToFill(ICrossError error)
    {
        if (FieldDisplayName is not null)
        {
            return FieldDisplayName;
        }

        if (error.FieldDisplayName is not null)
        {
            return error.FieldDisplayName;
        }

        if (error.FieldName is not null)
        {
            return error.FieldName;
        }

        return null;
    }
}