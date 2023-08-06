using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using CrossValidation.Exceptions;
using CrossValidation.Validators;
using CrossValidation.Validators.PredicateValidators;

namespace CrossValidation.Validations;

/// <summary>
/// To be used by the library users to validate data.
/// </summary>
public interface IValidation<out TField> : IValidationOperation
{
    [Pure]
    IValidation<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer);

    [Pure]
    IValidation<TField> WithCode(string code);

    [Pure]
    IValidation<TField> WithMessage(string message);
    
    [Pure]
    IValidation<TField> WithDetails(string details);

    [Pure]
    IValidation<TField> WithException(BusinessException exception);

    [Pure]
    IValidation<TField> WithFieldDisplayName(string fieldDisplayName);
    
    [Pure]
    IValidation<TField> WithHttpStatusCode(HttpStatusCode code);

    [Pure]
    IValidation<TField> When(Func<bool> condition);

    [Pure]
    IValidation<TField> When(Func<TField, bool> condition);

    [Pure]
    IValidation<TField> WhenAsync(Func<TField, Task<bool>> condition);

    IValidation<TField> Must(Func<TField, bool> condition);
    
    IValidation<TField> MustAsync(Func<TField, Task<bool>> condition);

    IValidation<TField> Must(Func<TField, BusinessException?> condition);

    IValidation<TField> MustAsync(Func<TField, Task<BusinessException?>> condition);

    [Pure]
    TField Instance();

    [Pure]
    TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance);

    IValidation<TField> SetValidator(Func<IValidator<BusinessException>> validator);

    IValidation<TField> SetAsyncValidator(Func<Task<IValidator<BusinessException>>> validator);

    IValidation<TField> SetScope(Action scope, ScopeType type);
    
    IValidation<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator);
    
    Task ValidateAsync();
    
    ValidationContext? Context { get; set; }

    TField GetFieldValue();

    static IValidation<TField> CreateFromFieldName(
        Func<TField>? getFieldValue,
        Type? customExceptionToThrow,
        string fieldName,
        ValidationContext? context,
        BusinessException? exception,
        string message,
        string? code,
        string? details,
        HttpStatusCode? statusCode,
        string? fieldDisplayName)
    {
        var fullPath = fieldName.Contains('.')
            ? fieldName.Substring(fieldName.IndexOf('.') + 1)
            : fieldName;
        return new Validation<TField>(
            getFieldValue: getFieldValue,
            customThrowToThrow: customExceptionToThrow,
            createGenericError: false,
            fieldPath: fullPath,
            context: null,
            index: null,
            parentPath: null,
            fixedException: exception,
            fixedMessage: message,
            fixedCode: code,
            fixedDetails: details,
            fixedStatusCode: statusCode,
            fixedFieldDisplayName: fieldDisplayName);
    }

    static IValidation<TField> CreateFailed()
    {
        return new Validation<TField>
        {
            HasFailed = true
        };
    }
}

internal class Validation<TField> :
    ValidationOperation,
    IValidation<TField>
{
    public IValidation<TField> SetValidator(Func<IValidator<BusinessException>> validator)
    {
        if (HasFailed)
        {
            return IValidation<TField>.CreateFailed();
        }
        
        Validator = validator;

        if (!HasPendingAsyncValidation)
        {
            ExecuteAsync(Context!, useAsync: false)
                .GetAwaiter()
                .GetResult();
        }

        return CreateNextValidation();
    }
    
    public IValidation<TField> SetAsyncValidator(Func<Task<IValidator<BusinessException>>> validator)
    {
        if (HasFailed)
        {
            return IValidation<TField>.CreateFailed();
        }
        
        AsyncValidator = validator;
        MarkAsPendingAsyncValidation();
        return CreateNextValidation();
    }
    
    public IValidation<TField> SetScope(Action scope, ScopeType type)
    {
        if (HasFailed)
        {
            return IValidation<TField>.CreateFailed();
        }
        
        Scope = scope;
        ScopeType = type;
    
        if (!HasPendingAsyncValidation)
        {
            ExecuteAsync(Context!, useAsync: false)
                .GetAwaiter()
                .GetResult();
        }
    
        return CreateNextValidation();
    }

    public IValidation<TField> WithCode(string code)
    {
        if (!HasFailed)
        {
            Code = Context!.Code ?? code;
        }

        return this;
    }

    public IValidation<TField> WithMessage(string message)
    {
        if (!HasFailed)
        {
            Message = Context!.Message != ""
                ? Context.Message
                : message;
        }
        
        return this;
    }
    
    public IValidation<TField> WithDetails(string details)
    {
        if (!HasFailed)
        {
            Details = Context!.Details ?? details;
        }
        
        return this;
    }

    public IValidation<TField> WithException(BusinessException exception)
    {
        if (!HasFailed)
        {
            var exceptionToAdd = Context!.Exception ?? exception;
            exception.CustomExceptionToThrow = CustomExceptionToThrow;
            TakeCustomizationsFromException(exceptionToAdd, Context);
            exception.GetFieldValue = GetNonGenericFieldValue;
            Exception = exceptionToAdd;
        }

        return this;
    }

    public IValidation<TField> WithFieldDisplayName(string fieldDisplayName)
    {
        if (!HasFailed)
        {
            FieldDisplayName = Context!.FieldDisplayName ?? fieldDisplayName;
        }
        
        return this;
    }

    public IValidation<TField> WithHttpStatusCode(HttpStatusCode code)
    {
        if (!HasFailed)
        {
            StatusCode = Context!.StatusCode ?? code;
        }
        
        return this;
    }

    public IValidation<TField> When(Func<bool> condition)
    {
        Condition = condition;
        return this;
    }

    public IValidation<TField> When(Func<TField, bool> condition)
    {
        Condition = () => condition(GetFieldValue());
        return this;
    }

    public IValidation<TField> WhenAsync(Func<TField, Task<bool>> condition)
    {
        HasPendingAsyncValidation = true;
        AsyncCondition = async () => await condition(GetFieldValue());
        return this;
    }

    public IValidation<TField> Must(Func<TField, bool> condition)
    {
        return SetValidator(() =>
        {
            var predicate = () => condition(GetFieldValue());
            return new BooleanPredicateValidator(predicate);
        });
    }
    
    public IValidation<TField> MustAsync(Func<TField, Task<bool>> condition)
    {
        return SetAsyncValidator(async () =>
        {
            var predicate = await condition(GetFieldValue());
            return new BooleanPredicateValidator(() => predicate);
        });
    }
    
    public IValidation<TField> Must(Func<TField, BusinessException?> condition)
    {
        if (!HasFailed)
        {
            var predicate = () =>
            {
                var exception = condition(GetFieldValue());

                if (exception is not null)
                {
                    WithException(exception);
                }

                return new ExceptionPredicateValidator(() => exception);
            };

            return SetValidator(predicate);
        }

        return this;
    }

    public IValidation<TField> MustAsync(Func<TField, Task<BusinessException?>> condition)
    {
        if (!HasFailed)
        {
            return SetAsyncValidator(async () =>
            {
                var exception = await condition(GetFieldValue());
                
                if (exception is not null)
                {
                    WithException(exception);
                }

                return new ExceptionPredicateValidator(() => exception);
            });
        }

        return this;
    }
    
    public IValidation<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer)
    {
        if (HasFailed)
        {
            return IValidation<TFieldTransformed>.CreateFailed();
        }
        
        var getFieldValueTransformed = () => transformer(GetFieldValue());
        var nextValidation = new Validation<TFieldTransformed>(
            getFieldValue: getFieldValueTransformed,
            customThrowToThrow: CustomExceptionToThrow,
            createGenericError: false,
            fieldPath: FieldPath,
            context: Context,
            index: Index,
            parentPath: ParentPath,
            fixedException: Context!.Exception ?? Exception,
            fixedMessage: Context!.Message ?? Message,
            fixedCode: Context!.Code ?? Code,
            fixedDetails: Context!.Details ?? Details,
            fixedStatusCode: Context!.StatusCode ?? StatusCode,
            fixedFieldDisplayName: Context!.FieldDisplayName ?? FieldDisplayName);
        nextValidation.HasFailed = HasFailed;
        nextValidation.HasPendingAsyncValidation = HasPendingAsyncValidation;
        nextValidation.ScopeType = ScopeType;
        nextValidation.IsInsideScope = true;
        nextValidation.ScopeCreatorValidation = ScopeCreatorValidation;
        NextValidation = nextValidation;
        return nextValidation;
    }

    public IValidation<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator)
    {
        validator.ScopeCreatorValidation = (IValidation<TChildModel>?)this;
        var oldContext = Context!;
        Context = Context!.CloneForChildModelValidator();
        validator._validationMode = Context.ValidationMode;
        var oldParentPath = ParentPath;
        var oldFieldPath = FieldPath;
        var oldFieldName = FieldName;

        SetScope(() =>
        {
            IsScopeCreator = true;
            validator.ScopeCreatorValidation = (IValidation<TChildModel>?)this;
            ParentPath = FieldName;
            FieldPath = null;
            FieldName = null;
            var childModel = (TChildModel)(object)GetFieldValue()!;
            validator.Model = childModel;
            validator.CreateValidations();
        }, Validations.ScopeType.ModelValidator);

        if (HasFailed)
        {
            return IValidation<TField>.CreateFailed();
        }
        
        var nextValidation = new Validation<TField>(
            getFieldValue: GetFieldValue,
            customThrowToThrow: CustomExceptionToThrow,
            createGenericError: CreateGenericError,
            fieldPath: oldFieldPath,
            context: oldContext,
            index: Index,
            parentPath: oldParentPath,
            fixedException: oldContext.Exception,
            fixedMessage: oldContext.Message,
            fixedCode: oldContext.Code,
            fixedDetails: oldContext.Details,
            fixedStatusCode: oldContext.StatusCode,
            fixedFieldDisplayName: oldContext.FieldDisplayName);
        nextValidation.FieldName = oldFieldName;
        nextValidation.HasFailed = HasFailed;
        nextValidation.HasPendingAsyncValidation = HasPendingAsyncValidation;
        nextValidation.ScopeType = ScopeType;
        nextValidation.IsInsideScope = IsInsideScope;
        nextValidation.ScopeCreatorValidation = ScopeCreatorValidation;
        NextValidation = nextValidation;
        return nextValidation;
    }
    
    public Func<TField>? GetGenericFieldValue { get; set; }
    public ValidationContext? Context { get; set; }

    public async Task ValidateAsync()
    {
        await Context!.ValidationTree!.TraverseAsync(Context);
    }

    public TField GetFieldValue()
    {
        return GetGenericFieldValue!();
    }

    public Validation()
    {
    }

    public Validation(
        Func<TField>? getFieldValue,
        Type? customThrowToThrow,
        bool createGenericError,
        string? fieldPath,
        ValidationContext? context,
        int? index,
        string? parentPath,
        BusinessException? fixedException,
        string fixedMessage,
        string? fixedCode,
        string? fixedDetails,
        HttpStatusCode? fixedStatusCode,
        string? fixedFieldDisplayName)
    {
        if (context != null)
        {
            Context = context;
        }
        else
        {
            Context = new ValidationContext();
        }
        
        Context.ValidationTree ??= this;
        GetGenericFieldValue = getFieldValue;
        GetNonGenericFieldValue = () => getFieldValue!()!;
        CustomExceptionToThrow = customThrowToThrow;
        CreateGenericError = createGenericError;
        Index = index;
        ParentPath = parentPath;
        FieldPath = fieldPath;
        var parentPathPathSeparator = parentPath is not null ? "." : null;

        if (parentPath is not null || fieldPath is not null)
        {
            FieldName = $"{parentPath}{parentPathPathSeparator}{fieldPath}";
        }
        
        Context.Exception = fixedException;
        Context.Message = fixedMessage;
        Context.Code = fixedCode;
        Context.Details = fixedDetails;
        Context.StatusCode = fixedStatusCode;
        Context.FieldDisplayName = fixedFieldDisplayName;
    }

    public TField Instance()
    {
        return GetGenericFieldValue!();
    }

    public TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        try
        {
            return fieldToInstance(GetGenericFieldValue!());
        }
        catch (BusinessException e)
        {
            e.FieldName = null;
            e.PlaceholderValues.Clear();
            e.CustomExceptionToThrow = CustomExceptionToThrow;
            TakeCustomizationsFromInstanceException(e, Context!);
            HandleException(e, Context!);
            throw new UnreachableException();
        }
    }
    
    private IValidation<TField> CreateNextValidation()
    {
        if (HasFailed)
        {
            return IValidation<TField>.CreateFailed();
        }
        
        var nextValidation = new Validation<TField>(
            getFieldValue: GetFieldValue,
            customThrowToThrow: CustomExceptionToThrow,
            createGenericError: CreateGenericError,
            fieldPath: FieldPath,
            context: Context,
            index: Index,
            parentPath: ParentPath,
            fixedException: Context!.Exception,
            fixedMessage: Context!.Message,
            fixedCode: Context!.Code,
            fixedDetails: Context!.Details,
            fixedStatusCode: Context!.StatusCode,
            fixedFieldDisplayName: Context!.FieldDisplayName);
        nextValidation.HasFailed = HasFailed;
        nextValidation.HasPendingAsyncValidation = HasPendingAsyncValidation;
        nextValidation.ScopeType = ScopeType;
        nextValidation.IsInsideScope = IsInsideScope;
        nextValidation.ScopeCreatorValidation = ScopeCreatorValidation;
        NextValidation = nextValidation;
        return nextValidation;
    }
}