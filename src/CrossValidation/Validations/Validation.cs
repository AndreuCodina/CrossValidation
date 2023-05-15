﻿using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Validators;

namespace CrossValidation.Validations;

// public interface IValidation
// public interface IValidation<out TField> : IValidation

// internal class ValidationOperation2
// {
//     internal void Foo()
//     {
//     }
// }

public interface IValidation<out TField>
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
    
    IValidation<TField> SetValidationScope(Func<bool> scope);
    
    IValidation<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator);
    
    Task ValidateAsync();
    
    ValidationContext? Context { get; set; }
    
    string? FieldFullPath { get; set; }
    
    Type? CrossErrorToException { get; set; }
    
    bool HasFailed { get; set; }

    Func<object>? GetNonGenericFieldValue { get; set; }
    
    TField GetFieldValue();

    static IValidation<TField> CreateFromFieldName(
        Func<TField>? fieldValue,
        Type? crossErrorToException,
        string fieldName,
        bool allowFieldNameWithoutModel,
        ValidationContext? context = null,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null)
    {
        // if (!allowFieldNameWithoutModel)
        // {
        //     if (!fieldName.Contains("."))
        //     {
        //         throw new ArgumentException("Use Field without a model is not allowed");
        //     }
        // }

        throw new NotImplementedException();

        // var fieldFullPath = allowFieldNameWithoutModel
        //     ? fieldName
        //     : fieldName.Substring(fieldName.IndexOf('.') + 1);
        // return new Validation<TField>(
        //     getFieldValue: fieldValue,
        //     crossErrorToException: crossErrorToException,
        //     generalizeError: false,
        //     
        //     fieldFullPath: fieldFullPath,
        //     context: context,
        //     error: error,
        //     message: message,
        //     code: code,
        //     details: details,
        //     httpStatusCode: httpStatusCode,
        //     fieldDisplayName: fieldDisplayName);
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
            var isExecutionValid = ExecuteAsync(Context!, useAsync: false)
                .GetAwaiter()
                .GetResult();

            if (!isExecutionValid)
            {
                return IValidation<TField>.CreateFailed();
            }
        }

        var nextValidation = CreateNextValidation();
        return nextValidation;
    }

    public IValidation<TField> SetAsyncValidator(Func<Task<IValidator<ICrossError>>> validator)
    {
        if (HasFailed)
        {
            return IValidation<TField>.CreateFailed();
        }
        
        HasPendingAsyncValidation = true;
        AsyncValidator = validator;
        var nextValidation = CreateNextValidation();
        return nextValidation;
    }

    public IValidation<TField> SetValidationScope(Func<bool> scope)
    {
        throw new NotImplementedException();
        // if (!HasFailed)
        // {
        //     ValidationScope = scope;
        //
        //     if (Context.ValidationOperationsCollected.Any())
        //     {
        //         Context.ValidationOperationsCollected
        //             .Add(Context.CurrentOperation);
        //     }
        //     else
        //     {
        //         var isExecutionValid = Context
        //             .CurrentOperation
        //             .ExecuteAsync(Context, useAsync: false)
        //             .GetAwaiter()
        //             .GetResult();
        //
        //         if (!isExecutionValid)
        //         {
        //             return IValidation<TField>.CreateFailed();
        //         }
        //     }
        //
        //     Context.CurrentOperation = new ValidationOperation();
        // }
        //
        // return this;
    }

    public IValidation<TField> WithCode(string code)
    {
        if (!HasFailed)
        {
            Code = code;
        }

        return this;
    }
    
    public IValidation<TField> WithMessage(string message)
    {
        if (!HasFailed)
        {
            Message = message;
        }

        return this;
    }
    
    public IValidation<TField> WithDetails(string details)
    {
        if (!HasFailed)
        {
            Details = details;
        }

        return this;
    }

    public IValidation<TField> WithError(ICrossError error)
    {
        if (!HasFailed)
        {
            error.CrossErrorToException = CrossErrorToException;
            TakeCustomizationsFromError(error);
            Error = error;
        }

        return this;
    }

    public IValidation<TField> WithFieldDisplayName(string fieldDisplayName)
    {
        if (!HasFailed)
        {
            FieldDisplayName = fieldDisplayName;
        }

        return this;
    }

    public IValidation<TField> WithHttpStatusCode(HttpStatusCode code)
    {
        if (!HasFailed)
        {
            HttpStatusCode = code;
        }

        return this;
    }

    public IValidation<TField> When(Func<bool> condition)
    {
        if (!HasFailed)
        {
            Condition = condition;
        }

        return this;
    }

    public IValidation<TField> When(Func<TField, bool> condition)
    {
        if (!HasFailed)
        {
            Condition = () => condition(GetFieldValue());
        }

        return this;
    }

    public IValidation<TField> WhenAsync(Func<TField, Task<bool>> condition)
    {
        HasPendingAsyncValidation = true;
        // TODO: Assign AsyncCondition
        
        if (!HasFailed)
        {
            var predicate = () => condition(GetFieldValue())
                .GetAwaiter()
                .GetResult();
            Condition = predicate;
        }

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
                    // TODO
                    
                    // if (Context.ValidationOperationsCollected.Any())
                    // {
                    //     Context!.CurrentOperation =
                    //         Context.ValidationOperationsCollected[0];
                    // }
                    //
                    // WithError(error);
                    // Context!.CurrentOperation = new ValidationOperation();
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
                    // TODO
                    
                    // Context!.CurrentOperation = Context.ValidationOperationsCollected[0];
                    // WithError(error);
                    // Context.CurrentOperation = new ValidationOperation();
                }

                return new ErrorPredicateValidator(() => error);
            });
        }

        return this;
    }
    
    public IValidation<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer)
    {
        if (!HasFailed)
        {
            var getFieldValueTransformed = () => transformer(GetFieldValue());
            return new Validation<TFieldTransformed>(
                getFieldValueTransformed,
                CrossErrorToException,
                generalizeError: Context!.GeneralizeError,
                parentValidation: null, // TODO
                fieldFullPath: Context.FieldName,
                context: Context,
                // index: Index // Not required because the field name already contains the index
                error: Context.Error,
                message: Context.Message,
                code: Context.Code,
                details: Context.Details,
                httpStatusCode: Context.HttpStatusCode,
                fieldDisplayName: Context.FieldDisplayName);
        }

        return IValidation<TFieldTransformed>.CreateFailed();
    }

    public IValidation<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator)
    {
        if (!HasFailed)
        {
            // TODO: validValidation.Clean(); // Ignore customizations for model validators

            // TODO: Don't check the condition here and create or accumulate a ValidationOperation (a new one or a scope if it's possible)
            if (Condition is null || Condition())
            {
                // validValidation.Clean(); // Ignore customizations for model validators
                
                var oldContext = Context;
                var childContext = Context!.CloneForChildModelValidator(FieldFullPath);
                validator.Context = childContext;
                var childModel = (TChildModel)(object)GetFieldValue()!;
                validator.Validate(childModel);
                var newErrors = validator.Context.ErrorsCollected;
                validator.Context = oldContext;
                validator.Context!.ErrorsCollected = newErrors;
            }
        }

        return this;
    }

    public async Task ValidateAsync()
    {
        await Context!.ValidationTree!.TraverseAsync(Context);
    }

    // public async ValueTask<bool> ExecuteAsync(ValidationContext context, bool useAsync)
    // {
    //     if (Condition is not null)
    //     {
    //         if (!Condition())
    //         {
    //             return true;
    //         }
    //     }
    //     else if (AsyncCondition is not null)
    //     {
    //         if (!useAsync)
    //         {
    //             throw new InvalidOperationException("An asynchronous condition cannot be used in synchronous mode");
    //         }
    //         
    //         if (!await AsyncCondition())
    //         {
    //             return true;
    //         }
    //     }
    //     
    //     if (Validator is not null)
    //     {
    //         var error = Validator!().GetError();
    //
    //         if (error is null)
    //         {
    //             return true;
    //         }
    //
    //         // TODO
    //         // error.CrossErrorToException = CrossErrorToException;
    //         HandleError(error, context);
    //         // validValidation.Clean();
    //         return false;
    //     }
    //     else if (AsyncValidator is not null)
    //     {
    //         if (!useAsync)
    //         {
    //             throw new InvalidOperationException("An asynchronous validator cannot be used in synchronous mode");
    //         }
    //         
    //         var error = (await AsyncValidator!()).GetError();
    //
    //         if (error is null)
    //         {
    //             return true;
    //         }
    //
    //         // TODO
    //         // error.CrossErrorToException = CrossErrorToException;
    //         HandleError(error, context);
    //         // validValidation.Clean();
    //         return false;
    //     }
    //     else if (ValidationScope is not null)
    //     {
    //         return ValidationScope();
    //     }
    //
    //     HasBeenExecuted = true;
    //     return true;
    //     // return await ValueTask.FromResult(true);
    // }
    
    public Func<TField>? GetFieldValueTransformed { get; set; }
    // public List<ValidationOperation> ValidationOperationsCollected { get; set; } = new();
    // public ValidationOperation ValidationOperation { get; set; } = new();
    public ValidationContext? Context { get; set; }
    public string? FieldFullPath { get; set; }
    public Type? CrossErrorToException { get; set; }

    public TField GetFieldValue()
    {
        return (TField)GetNonGenericFieldValue!();
    }
    // public ValidationOperation ParentValidation { get; set; }

    public Validation()
    {
    }

    public Validation(
        Func<TField>? getFieldValue,
        Type? crossErrorToException,
        ValidationOperation? parentValidation,
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

        // if (parentValidation is not null)
        // {
        //     parentValidation.NextValidation = this;
        // }
        // else
        // {
        //     Context.ValidationTree.NextValidation = this;
        // }
        if (Context.ValidationTree is null)
        {
            Context.ValidationTree = this;
        }

        GetFieldValueTransformed = getFieldValue;
        GetNonGenericFieldValue = () => getFieldValue!()!;
        CrossErrorToException = crossErrorToException;
        FieldFullPath = fieldFullPath;
        Context.GeneralizeError = Context.IsChildContext && Context.GeneralizeError
            ? true
            : generalizeError;

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
        return GetFieldValueTransformed!();
    }

    public TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        try
        {
            return fieldToInstance(GetFieldValueTransformed!());
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
    
    private IValidation<TField> CreateNextValidation()
    {
        var nextValidation = new Validation<TField>(
            getFieldValue: GetFieldValue,
            crossErrorToException: CrossErrorToException,
            parentValidation: this,
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
        NextValidation = nextValidation;
        return nextValidation;
    }
}