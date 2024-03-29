using CrossValidation.ErrorResources;
using CrossValidation.Exceptions;
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
    Func<Validator>? Validator { get; set; }
    Func<Task<Validator>>? AsyncValidator { get; set; }
    Action? Scope { get; set; }
    string? Code { get; set; }
    string Message { get; set; }
    string? Details { get; set; }
    BusinessException? Exception { get; set; }
    string? FieldDisplayName { get; set; }
    int? StatusCode { get; set; }
    Type? CustomExceptionToThrow { get; set; }
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
    public bool CreateGenericError { get; set; }
    public ScopeType? ScopeType { get; set; }
    public string? ParentPath { get; set; }
    public string? FieldPath { get; set; }
    public string? FieldName { get; set; }
    ValueTask TraverseAsync(ValidationContext context);
    void MarkAllDescendantValidationsAsNotPendingAsync();
    ValueTask ExecuteAsync(ValidationContext context, bool useAsync);
    void HandleException(BusinessException exception, ValidationContext context);
    void TakeCustomizationsFromInstanceException(BusinessException exception, ValidationContext context);
    void TakeCustomizationsFromException(BusinessException exception, ValidationContext context);
    void MarkAsPendingAsyncValidation();
    void MarkAsFailed();
    
}

internal class ValidationOperation
{
    public Func<object>? GetNonGenericFieldValue { get; set; }
    public Func<Validator>? Validator { get; set; }
    public Func<Task<Validator>>? AsyncValidator { get; set; }
    public Action? Scope { get; set; }
    public string? Code { get; set; }
    public string Message { get; set; } = "";
    public string? Details { get; set; }
    public BusinessException? Exception { get; set; }
    public string? FieldDisplayName { get; set; }
    public int? StatusCode { get; set; }
    public Type? CustomExceptionToThrow { get; set; }
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
    public bool CreateGenericError { get; set; }
    public ScopeType? ScopeType { get; set; }
    public string? ParentPath { get; set; }
    public string? FieldPath { get; set; }
    public string? FieldName { get; set; }

    public async ValueTask TraverseAsync(ValidationContext context)
    {
        bool StopExecution()
        {
            var isModelValidator = ScopeType is not null && ScopeType is Validations.ScopeType.ModelValidator;
            var stopForEach = (ScopeType is not null && ScopeType is Validations.ScopeType.ForEach && context.ValidationMode is not ValidationMode.AccumulateFirstErrorRelatedToFieldAndFirstErrorOfAllIterations);
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
            var exception = Validator!().GetException();

            if (exception is null)
            {
                return;
            }
            
            HandleException(exception, context);
            MarkAsFailed();
        }
        else if (AsyncValidator is not null)
        {
            if (!useAsync)
            {
                throw new InvalidOperationException("An asynchronous validator cannot be used in synchronous mode");
            }
            
            var exception = (await AsyncValidator()).GetException();

            if (exception is null)
            {
                return;
            }
            
            HandleException(exception, context);
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
        var exceptionToAdd = context.Exception ?? (Exception ?? exception);
        AddException(exceptionToAdd, context);

        if (context.ValidationMode is ValidationMode.StopOnFirstError
            && context.ExceptionsCollected.Count == 1)
        {
            if (CustomExceptionToThrow is not null)
            {
                throw (Exception)Activator.CreateInstance(
                    CustomExceptionToThrow,
                    CreateParametrizedExceptionMessage(context.ExceptionsCollected[0]))!;
            }
                
            throw context.ExceptionsCollected[0];
        }
    }
    
    private string CreateParametrizedExceptionMessage(BusinessException exception)
    {
        return $"{FieldName}: {exception.Message}";
    }
    
    public void TakeCustomizationsFromInstanceException(BusinessException exception, ValidationContext context)
    {
        if (CreateGenericError)
        {
            return;
        }

        var codeToAdd = Code ?? exception.Code;
        var isInstanceCallerWithCodeAndWithoutMessage = Code is not null && Message == "";
        
        if (isInstanceCallerWithCodeAndWithoutMessage && codeToAdd is not null)
        {
            Message = CrossValidationOptions.GetMessageFromCode(codeToAdd);
        }
        else
        {
            Message = Message != ""
                ? Message
                : exception.Message;
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

        if (context.StatusCode is not null)
        {
            StatusCode = (int)context.StatusCode;
        }
        else
        {
            StatusCode = exception.StatusCode;
        }
        
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
        exception.AddCommonPlaceholderValues();
        context.ExceptionsCollected.Add(exception);
    }

    private void AddCustomizationsToException(BusinessException exception, ValidationContext context)
    {
        exception.CustomExceptionToThrow = CustomExceptionToThrow;
        exception.FieldName = FieldName;
        exception.FieldDisplayName = GetFieldDisplayNameToFill(exception, context);
        exception.GetFieldValue = GetNonGenericFieldValue;
        exception.Code = GetCodeToFill(exception, context);
        exception.FormattedMessage = GetMessageToFill(exception, context);
        exception.Details = context.Details ?? (Details ?? exception.Details);

        if (context.StatusCode is not null)
        {
            exception.StatusCode = context.StatusCode.Value;
        }
        else if (StatusCode is not null)
        {
            exception.StatusCode = StatusCode.Value;
        }
        else
        {
            exception.StatusCode = exception.StatusCode;
        }
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
        
        if (CreateGenericError)
        {
            return nameof(ErrorResource.Generic);
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
        
        if (CreateGenericError)
        {
            return CrossValidationOptions.GetMessageFromCode(nameof(ErrorResource.Generic));
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

    private string? GetFieldDisplayNameToFill(BusinessException exception, ValidationContext context)
    {
        if (context.FieldDisplayName is not null)
        {
            return context.FieldDisplayName;
        }
        
        if (FieldDisplayName is not null)
        {
            return FieldDisplayName;
        }

        if (exception.FieldDisplayName is not null)
        {
            return exception.FieldDisplayName;
        }

        if (exception.FieldName is not null)
        {
            return exception.FieldName;
        }

        return null;
    }
}