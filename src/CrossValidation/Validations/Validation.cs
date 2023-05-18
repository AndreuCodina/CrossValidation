using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Validators;

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
    IValidation<TField> WithError(ICrossError error);

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

    IValidation<TField> Must(Func<TField, ICrossError?> condition);

    IValidation<TField> MustAsync(Func<TField, Task<ICrossError?>> condition);

    [Pure]
    TField Instance();
    
    [Pure]
    TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance);

    IValidation<TField> SetValidator(Func<IValidator<ICrossError>> validator);

    IValidation<TField> SetAsyncValidator(Func<Task<IValidator<ICrossError>>> validator);

    IValidation<TField> SetScope(Action scope);
    
    IValidation<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator);
    
    Task ValidateAsync();
    
    ValidationContext? Context { get; set; }
    
    string? FieldFullPath { get; set; }
    
    TField GetFieldValue();

    IValidation<TField> CreateNextValidation();

    static IValidation<TField> CreateFromFieldName(
        Func<TField>? getFieldValue,
        Type? crossErrorToException,
        string fieldName,
        ValidationContext? context = null,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null)
    {
        var fieldFullPath = fieldName.Contains('.')
            ? fieldName.Substring(fieldName.IndexOf('.') + 1)
            : fieldName;
        return new Validation<TField>(
            getFieldValue: getFieldValue,
            crossErrorToException: crossErrorToException,
            generalizeError: false,
            fieldFullPath: fieldFullPath,
            error: error,
            message: message,
            code: code,
            details: details,
            httpStatusCode: httpStatusCode,
            fieldDisplayName: fieldDisplayName);
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
    public IValidation<TField> SetValidator(Func<IValidator<ICrossError>> validator)
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
    
    public IValidation<TField> SetAsyncValidator(Func<Task<IValidator<ICrossError>>> validator)
    {
        if (HasFailed)
        {
            return IValidation<TField>.CreateFailed();
        }
        
        AsyncValidator = validator;
        MarkAsPendingAsyncValidation();
        return CreateNextValidation();
    }
    
    public IValidation<TField> SetScope(Action scope)
    {
        if (HasFailed)
        {
            return IValidation<TField>.CreateFailed();
        }
        
        Scope = scope;
    
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
        Code = code;
        return this;
    }
    
    public IValidation<TField> WithMessage(string message)
    {
        Message = message;
        return this;
    }
    
    public IValidation<TField> WithDetails(string details)
    {
        Details = details;
        return this;
    }

    public IValidation<TField> WithError(ICrossError error)
    {
        if (!HasFailed)
        {
            error.CrossErrorToException = CrossErrorToException;
            TakeCustomizationsFromError(error);
            error.GetFieldValue = GetNonGenericFieldValue;
            Error = error;
        }

        return this;
    }

    public IValidation<TField> WithFieldDisplayName(string fieldDisplayName)
    {
        FieldDisplayName = fieldDisplayName;
        return this;
    }

    public IValidation<TField> WithHttpStatusCode(HttpStatusCode code)
    {
        HttpStatusCode = code;
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
    
    public IValidation<TField> Must(Func<TField, ICrossError?> condition)
    {
        if (!HasFailed)
        {
            var predicate = () =>
            {
                var error = condition(GetFieldValue());

                if (error is not null)
                {
                    WithError(error);
                }

                return new ErrorPredicateValidator(() => error);
            };

            return SetValidator(predicate);
        }

        return this;
    }

    public IValidation<TField> MustAsync(Func<TField, Task<ICrossError?>> condition)
    {
        if (!HasFailed)
        {
            return SetAsyncValidator(async () =>
            {
                var error = await condition(GetFieldValue());
                
                if (error is not null)
                {
                    WithError(error);
                }

                return new ErrorPredicateValidator(() => error);
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
            crossErrorToException: CrossErrorToException,
            generalizeError: false,
            fieldFullPath: FieldFullPath,
            context: Context,
            index: Index,
            parentPath: ParentPath,
            error: null,
            message: null,
            code: null,
            details: null,
            httpStatusCode: null,
            fieldDisplayName: null);
        nextValidation.HasFailed = HasFailed;
        nextValidation.HasPendingAsyncValidation = HasPendingAsyncValidation;
        nextValidation.IsInsideScope = true;
        nextValidation.ScopeCreatorValidation = ScopeCreatorValidation;
        NextValidation = nextValidation;
        return nextValidation;
    }

    public IValidation<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator)
    {
        validator.ScopeCreatorValidation = (IValidation<TChildModel>?)this;
        Context = Context!.CloneForChildModelValidator(FieldFullPath);
        var nextValidation = SetScope(() =>
        {
            var childModel = (TChildModel)(object)GetFieldValue()!;
            validator.CreateValidations(childModel);
        });
        return nextValidation;
    }
    
    public Func<TField>? GetGenericFieldValue { get; set; }
    public ValidationContext? Context { get; set; }
    public string? FieldFullPath { get; set; }

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
        Type? crossErrorToException,
        bool generalizeError,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null)
    {
        if (context != null)
        {
            Context = context;
        }
        else
        {
            Context = new ValidationContext();
        }
        
        if (Context.ValidationTree is null)
        {
            Context.ValidationTree = this;
        }

        GetGenericFieldValue = getFieldValue;
        GetNonGenericFieldValue = () => getFieldValue!()!;
        CrossErrorToException = crossErrorToException;
        FieldFullPath = fieldFullPath;
        GeneralizeError = generalizeError;
        Index = index;
        var indexRepresentation = Context.FieldName is not null && index is not null
            ? $"[{index}]"
            : null;
        ParentPath = parentPath;
        string? parentPathValue = null;

        if (parentPath is not null)
        {
            parentPathValue = parentPath;
        }
        else if (Context.ParentPath is not null)
        {
            parentPathValue = Context.ParentPath;
        }

        if (parentPathValue is not null)
        {
            parentPathValue += ".";
        }

        Context.FieldName = parentPathValue + fieldFullPath + indexRepresentation;

        if (Context.FieldName is "")
        {
            Context.FieldName = null;
        }

        Context.Error = error;
        Context.Message = message;
        Context.Code = code;
        Context.Details = details;
        Context.HttpStatusCode = httpStatusCode;
        Context.FieldDisplayName = fieldDisplayName;
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
        catch (CrossException e)
        {
            e.Error.FieldName = null;
            e.Error.PlaceholderValues = null;
            e.Error.CrossErrorToException = CrossErrorToException;
            TakeCustomizationsFromInstanceError(e.Error, Context!);
            HandleError(e.Error, Context!);
            throw new UnreachableException();
        }
    }
    
    public IValidation<TField> CreateNextValidation()
    {
        if (HasFailed)
        {
            return IValidation<TField>.CreateFailed();
        }
        
        var nextValidation = new Validation<TField>(
            getFieldValue: GetFieldValue,
            crossErrorToException: CrossErrorToException,
            generalizeError: GeneralizeError,
            fieldFullPath: FieldFullPath,
            context: Context,
            index: Index,
            parentPath: ParentPath,
            error: null,
            message: null,
            code: null,
            details: null,
            httpStatusCode: null,
            fieldDisplayName: null);
        nextValidation.HasFailed = HasFailed;
        nextValidation.HasPendingAsyncValidation = HasPendingAsyncValidation;
        nextValidation.IsInsideScope = IsInsideScope;
        nextValidation.ScopeCreatorValidation = ScopeCreatorValidation;
        NextValidation = nextValidation;
        return nextValidation;
    }
}