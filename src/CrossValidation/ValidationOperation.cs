using System.Net;
using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.Validations;
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
    Action? Scope { get; set; }
    string? Code { get; set; }
    string? Message { get; set; }
    string? Details { get; set; }
    ICrossError? Error { get; set; }
    string? FieldDisplayName { get; set; }
    HttpStatusCode? HttpStatusCode { get; set; }
    Type? CrossErrorToException { get; set; }
    Func<bool>? Condition { get; set; }
    Func<Task<bool>>? AsyncCondition { get; set; }
    IValidationOperation? NextValidation { get; set; }
    bool HasFailed { get; set; }
    bool HasBeenExecuted { get; set; }
    bool HasPendingAsyncValidation { get; set; }
    bool IsScopeCreator { get; set; }
    List<IValidationOperation>? ScopeValidations { get; set; }
    int? Index { get; set; }
    public bool IsInsideScope { get; set; }
    public IValidationOperation? ScopeCreatorValidation { get; set; }
    public bool GeneralizeError { get; set; }
    public ScopeType? ScopeType { get; set; }
    public string? ParentPath { get; set; }
    public string? FieldPath { get; set; }
    public string? FieldName { get; set; }
    ValueTask TraverseAsync(ValidationContext context);
    void MarkAllDescendantValidationsAsNotPendingAsync();
    ValueTask ExecuteAsync(ValidationContext context, bool useAsync);
    void HandleError(ICrossError error, ValidationContext context);
    void TakeCustomizationsFromInstanceError(ICrossError error, ValidationContext context);
    void TakeCustomizationsFromError(ICrossError error, ValidationContext context);
    void MarkAsPendingAsyncValidation();
    void MarkAsFailed();
    
}

internal class ValidationOperation
{
    public Func<object>? GetNonGenericFieldValue { get; set; }
    public Func<IValidator<ICrossError>>? Validator { get; set; }
    public Func<Task<IValidator<ICrossError>>>? AsyncValidator { get; set; }
    public Action? Scope { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public ICrossError? Error { get; set; }
    public string? FieldDisplayName { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    public Type? CrossErrorToException { get; set; }
    public Func<bool>? Condition { get; set; }
    public Func<Task<bool>>? AsyncCondition { get; set; }
    public IValidationOperation? NextValidation { get; set; }
    public bool HasFailed { get; set; }
    public bool HasBeenExecuted { get; set; }
    public bool HasPendingAsyncValidation { get; set; }
    public bool IsScopeCreator { get; set; }
    public List<IValidationOperation>? ScopeValidations { get; set; }
    public int? Index { get; set; }
    public bool IsInsideScope { get; set; }
    public IValidationOperation? ScopeCreatorValidation { get; set; }
    public bool GeneralizeError { get; set; }
    public ScopeType? ScopeType { get; set; }
    public string? ParentPath { get; set; }
    public string? FieldPath { get; set; }
    public string? FieldName { get; set; }

    public async ValueTask TraverseAsync(ValidationContext context)
    {
        bool StopExecution()
        {
            var isModelValidator = ScopeType is not null && ScopeType is Validations.ScopeType.ModelValidator;
            var stopForEach = (ScopeType is not null && ScopeType is Validations.ScopeType.ForEach && context.ValidationMode is not ValidationMode.AccumulateFirstErrorsAndAllIterationFirstErrors);
            var stopWhenNotNull = (ScopeType is not null && ScopeType is Validations.ScopeType.WhenNotNull);
            return HasFailed
                    && (!IsScopeCreator || stopForEach || stopWhenNotNull)
                    && !isModelValidator;
        }
        if (StopExecution())
        {
            return;
        }
        
        if (!HasBeenExecuted)
        {
            await ExecuteAsync(context, useAsync: true);

            if (HasFailed)
            {
                return;
            }
        }
        
        if (IsScopeCreator && ScopeValidations is not null)
        {
            foreach (var scopeValidation in ScopeValidations)
            {
                do
                {
                    if (StopExecution())
                    {
                        break;
                    }
                    
                    MarkAllDescendantValidationsAsNotPendingAsync();
                    await scopeValidation.TraverseAsync(context);
                } while (scopeValidation.HasPendingAsyncValidation);
            }
        }

        if (!HasFailed && NextValidation is not null)
        {
            await NextValidation.TraverseAsync(context);
        }
    }

    public void MarkAllDescendantValidationsAsNotPendingAsync()
    {
        HasPendingAsyncValidation = false;
        
        if (ScopeValidations is not null)
        {
            foreach (var scopeValidation in ScopeValidations)
            {
                scopeValidation.MarkAllDescendantValidationsAsNotPendingAsync();
            }
        }
        
        if (NextValidation is not null)
        {
            NextValidation.MarkAllDescendantValidationsAsNotPendingAsync();
        }
    }

    public async ValueTask ExecuteAsync(ValidationContext context, bool useAsync)
    {
        if (Condition is not null)
        {
            if (!Condition())
            {
                return;
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
                return;
            }
        }
        
        if (Validator is not null)
        {
            var error = Validator!().GetError();

            if (error is null)
            {
                return;
            }
            
            HandleError(error, context);
            MarkAsFailed();
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
                return;
            }
            
            HandleError(error, context);
            MarkAsFailed();
        }
        else if (Scope is not null)
        {
            Scope();
        }

        HasBeenExecuted = true;
    }

    public void HandleError(ICrossError error, ValidationContext context)
    {
        var errorToAdd = context.Error ?? (Error ?? error);
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
        if (GeneralizeError)
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
    
    public void TakeCustomizationsFromError(ICrossError error, ValidationContext context)
    {
        Code = context.Code ?? (error.Code ?? Code);
        Message = context.Message ?? (error.Message ?? Message);
        Details = context.Details ?? (error.Details ?? Details);
        HttpStatusCode = context.HttpStatusCode ?? (error.HttpStatusCode ?? HttpStatusCode);
        FieldDisplayName = context.FieldDisplayName ?? (error.FieldDisplayName ?? FieldDisplayName);
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
        context.ErrorsCollected.Add(error);
    }

    private void AddCustomizationsToError(ICrossError error, ValidationContext context)
    {
        error.CrossErrorToException = CrossErrorToException;
        error.FieldName = FieldName;
        error.FieldDisplayName = GetFieldDisplayNameToFill(error, context);
        error.GetFieldValue = GetNonGenericFieldValue;
        error.Code = GetCodeToFill(error, context);
        error.Message = GetMessageToFill(error, context);
        error.Details = context.Details ?? (Details ?? error.Details);
        error.HttpStatusCode = context.HttpStatusCode ?? (HttpStatusCode ?? error.HttpStatusCode);
    }

    private string? GetCodeToFill(ICrossError error, ValidationContext context)
    {
        if (context.Code is not null)
        {
            return context.Code;
        }
        
        if (Code is not null)
        {
            return Code;
        }
        
        if (GeneralizeError)
        {
            return nameof(ErrorResource.General);
        }

        return error.Code;
    }

    private string? GetMessageToFill(ICrossError error, ValidationContext context)
    {
        if (context.Message is not null)
        {
            return context.Message;
        }
        
        if (Message is not null)
        {
            return Message;
        }

        if (Code is not null)
        {
            return CrossValidationOptions.GetMessageFromCode(Code);
        }
        
        if (GeneralizeError)
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

    private string? GetFieldDisplayNameToFill(ICrossError error, ValidationContext context)
    {
        if (context.FieldDisplayName is not null)
        {
            return context.FieldDisplayName;
        }
        
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