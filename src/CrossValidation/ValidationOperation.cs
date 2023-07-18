using System.Net;
using CrossValidation.Exceptions;
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
    Func<IValidator<BusinessException>>? Validator { get; set; }
    Func<Task<IValidator<BusinessException>>>? AsyncValidator { get; set; }
    Action? Scope { get; set; }
    string? Code { get; set; }
    string Message { get; set; }
    string? Details { get; set; }
    BusinessException? Exception { get; set; }
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
    void HandleException(BusinessException exception, ValidationContext context);
    void TakeCustomizationsFromInstanceException(BusinessException exception, ValidationContext context);
    void TakeCustomizationsFromException(BusinessException error, ValidationContext context);
    void MarkAsPendingAsyncValidation();
    void MarkAsFailed();
    
}

internal class ValidationOperation
{
    public Func<object>? GetNonGenericFieldValue { get; set; }
    public Func<IValidator<BusinessException>>? Validator { get; set; }
    public Func<Task<IValidator<BusinessException>>>? AsyncValidator { get; set; }
    public Action? Scope { get; set; }
    public string? Code { get; set; }
    public string Message { get; set; } = "";
    public string? Details { get; set; }
    public BusinessException? Exception { get; set; }
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
            
            HandleException(error, context);
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
            
            HandleException(error, context);
            MarkAsFailed();
        }
        else if (Scope is not null)
        {
            Scope();
        }

        HasBeenExecuted = true;
    }

    public void HandleException(BusinessException exception, ValidationContext context)
    {
        var exceptionToAdd = context.Error ?? (Exception ?? exception);
        AddException(exceptionToAdd, context);

        if (context is {ValidationMode: ValidationMode.StopOnFirstError})
        {
            if (context.ExceptionsCollected.Count == 1)
            {
                throw context.ExceptionsCollected[0];
            }
        }
    }
    
    public void TakeCustomizationsFromInstanceException(BusinessException exception, ValidationContext context)
    {
        if (GeneralizeError)
        {
            return;
        }

        var codeToAdd = Code ?? exception.Code;
        var isInstanceCallerWithCodeAndWithoutMessage = Code is not null && Message is null;
        
        if (isInstanceCallerWithCodeAndWithoutMessage && codeToAdd is not null)
        {
            Message = CrossValidationOptions.GetMessageFromCode(codeToAdd);
        }
        else
        {
            Message ??= exception.Message;
        }

        Code = codeToAdd;
    }
    
    public void TakeCustomizationsFromException(BusinessException exception, ValidationContext context)
    {
        Code = context.Code ?? (exception.Code ?? Code);
        Message = context.Message != ""
            ? context.Message
            : exception.Message != "" ? exception.Message : Message;
        Details = context.Details ?? (exception.Details ?? Details);
        HttpStatusCode = context.HttpStatusCode ?? exception.StatusCode;
        FieldDisplayName = context.FieldDisplayName ?? (exception.FieldDisplayName ?? FieldDisplayName);
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
    
    private void AddException(BusinessException exception, ValidationContext context)
    {
        AddCustomizationsToException(exception, context);
        context.ExceptionsCollected.Add(exception);
    }

    private void AddCustomizationsToException(BusinessException exception, ValidationContext context)
    {
        exception.CrossErrorToException = CrossErrorToException;
        exception.FieldName = FieldName;
        exception.FieldDisplayName = GetFieldDisplayNameToFill(exception, context);
        exception.GetFieldValue = GetNonGenericFieldValue;
        exception.Code = GetCodeToFill(exception, context);
        exception.FormattedMessage = GetMessageToFill(exception, context);
        exception.Details = context.Details ?? (Details ?? exception.Details);
        exception.StatusCode = context.HttpStatusCode ?? (HttpStatusCode ?? exception.StatusCode);
    }

    private string? GetCodeToFill(BusinessException exception, ValidationContext context)
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

        return exception.Code;
    }

    private string GetMessageToFill(BusinessException exception, ValidationContext context)
    {
        if (context.Message != "")
        {
            return context.Message;
        }
        
        if (Message != "")
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

        if (exception.Message != "")
        {
            return exception.Message;
        }

        if (exception.Code is not null)
        {
            return CrossValidationOptions.GetMessageFromCode(exception.Code);
        }

        return "";
    }

    private string? GetFieldDisplayNameToFill(BusinessException error, ValidationContext context)
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