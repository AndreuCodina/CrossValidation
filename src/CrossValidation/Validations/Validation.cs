﻿using System.Diagnostics.Contracts;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Validators;

namespace CrossValidation.Validations;

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
    IValidation<TField> When(bool condition);

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

    Task RunAsync();
}

internal abstract class Validation<TField> : IValidation<TField>
{
    public IValidation<TField> SetValidator(Func<IValidator<ICrossError>> validator)
    {
        if (this is IValidValidation<TField> validValidation)
        {
            if (validValidation.Context.ValidationOperation!.ExecuteNextValidator)
            {
                validValidation.Context.ValidationOperation.Validator = validator;
                
                if (validValidation.Context.ValidationOperationsCollected.Any())
                {
                    validValidation.Context.ValidationOperationsCollected
                        .Add(validValidation.Context.ValidationOperation);
                }
                else
                {
                    var isExecutionValid = validValidation.Context
                        .ValidationOperation
                        .ExecuteAsync(validValidation.Context, useAsync: false)
                        .GetAwaiter()
                        .GetResult();
                    
                    if (!isExecutionValid)
                    {
                        return IInvalidValidation<TField>.Create();
                    }
                }
            }

            validValidation.Context.ValidationOperation = new ValidationOperation<TField>();
        }

        return this;
    }
    
    public IValidation<TField> SetAsyncValidator(Func<Task<IValidator<ICrossError>>> validator)
    {
        if (this is IValidValidation<TField> validValidation)
        {
            if (validValidation.Context.ValidationOperation!.ExecuteNextValidator)
            {
                validValidation.Context.ValidationOperation.AsyncValidator = validator;
                validValidation.Context.ValidationOperationsCollected
                    .Add(validValidation.Context.ValidationOperation);
            }

            validValidation.Context.ValidationOperation = new ValidationOperation<TField>();
        }

        return this;
    }

    public IValidation<TField> SetValidationScope(Func<bool> scope)
    {
        if (this is IValidValidation<TField> validValidation)
        {
            if (validValidation.Context.ValidationOperation!.ExecuteNextValidator)
            {
                validValidation.Context.ValidationOperation.ValidationScope = scope;
                
                if (validValidation.Context.ValidationOperationsCollected.Any())
                {
                    validValidation.Context.ValidationOperationsCollected
                        .Add(validValidation.Context.ValidationOperation);
                }
                else
                {
                    var isExecutionValid = validValidation.Context
                        .ValidationOperation.ExecuteAsync(validValidation.Context, useAsync: false)
                        .GetAwaiter()
                        .GetResult();
                    
                    if (!isExecutionValid)
                    {
                        return IInvalidValidation<TField>.Create();
                    }
                }
            }

            validValidation.Context.ValidationOperation = new ValidationOperation<TField>();
        }

        return this;
    }

    public IValidation<TField> WithCode(string code)
    {
        if (this is IValidValidation<TField> validValidation && validValidation.Context.ValidationOperation!.ExecuteNextValidator)
        {
            validValidation.Context.ValidationOperation.Code = code;
        }

        return this;
    }
    
    public IValidation<TField> WithMessage(string message)
    {
        if (this is IValidValidation<TField> validValidation && validValidation.Context.ValidationOperation!.ExecuteNextValidator)
        {
            validValidation.Context.ValidationOperation.Message = message;
        }

        return this;
    }
    
    public IValidation<TField> WithDetails(string details)
    {
        if (this is IValidValidation<TField> validValidation && validValidation.Context.ValidationOperation!.ExecuteNextValidator)
        {
            validValidation.Context.ValidationOperation.Details = details;
        }

        return this;
    }

    public IValidation<TField> WithError(ICrossError error)
    {
        if (this is IValidValidation<TField> validValidation && validValidation.Context.ValidationOperation!.ExecuteNextValidator)
        {
            error.CrossErrorToException = validValidation.CrossErrorToException;
            validValidation.Context.ValidationOperation.TakeCustomizationsFromError(error);
            validValidation.Context.ValidationOperation.Error = error;
        }

        return this;
    }

    public IValidation<TField> WithFieldDisplayName(string fieldDisplayName)
    {
        if (this is IValidValidation<TField> validValidation && validValidation.Context.ValidationOperation!.ExecuteNextValidator)
        {
            validValidation.Context.ValidationOperation.FieldDisplayName = fieldDisplayName;
        }

        return this;
    }

    public IValidation<TField> WithHttpStatusCode(HttpStatusCode code)
    {
        if (this is IValidValidation<TField> validValidation && validValidation.Context.ValidationOperation!.ExecuteNextValidator)
        {
            validValidation.Context.ValidationOperation.HttpStatusCode = code;
        }

        return this;
    }

    public IValidation<TField> When(bool condition)
    {
        if (this is IValidValidation<TField> validValidation && validValidation.Context.ValidationOperation!.ExecuteNextValidator)
        {
            validValidation.Context.ValidationOperation.ExecuteNextValidator = condition;
        }

        return this;
    }

    public IValidation<TField> When(Func<TField, bool> condition)
    {
        if (this is IValidValidation<TField> validValidation && validValidation.Context.ValidationOperation!.ExecuteNextValidator)
        {
            validValidation.Context.ValidationOperation.ExecuteNextValidator = condition(validValidation.GetFieldValue());
        }

        return this;
    }

    public IValidation<TField> WhenAsync(Func<TField, Task<bool>> condition)
    {
        if (this is IValidValidation<TField> validValidation && validValidation.Context.ValidationOperation!.ExecuteNextValidator)
        {
            validValidation.Context.ValidationOperation.ExecuteNextValidator = condition(validValidation.GetFieldValue())
                .GetAwaiter()
                .GetResult();
        }

        return this;
    }

    public IValidation<TField> Must(Func<TField, bool> condition)
    {
        if (this is IValidValidation<TField> validValidation)
        {
            var conditionResult = () => condition(validValidation.GetFieldValue());
            return SetValidator(() => new BooleanPredicateValidator(conditionResult));
        }

        return this;
    }
    
    public IValidation<TField> MustAsync(Func<TField, Task<bool>> condition)
    {
        if (this is IValidValidation<TField> validValidation)
        {
            return SetAsyncValidator(async () =>
            {
                var predicate = await condition(validValidation.GetFieldValue());
                return new BooleanPredicateValidator(() => predicate);
            });
        }

        return this;
    }
    
    public IValidation<TField> Must(Func<TField, ICrossError?> condition)
    {
        if (this is IValidValidation<TField> validValidation)
        {
            var predicateValidator = () =>
            {
                var error = condition(validValidation.GetFieldValue());

                if (error is not null)
                {
                    if (validValidation.Context.ValidationOperationsCollected.Any())
                    {
                        validValidation.Context.ValidationOperation =
                            validValidation.Context.ValidationOperationsCollected[0];
                    }

                    validValidation.WithError(error);
                    validValidation.Context.ValidationOperation = new ValidationOperation<TField>();
                }

                return new ErrorPredicateValidator(() => error);
            };

            return SetValidator(predicateValidator);
        }

        return this;
    }

    public IValidation<TField> MustAsync(Func<TField, Task<ICrossError?>> condition)
    {
        if (this is IValidValidation<TField> validValidation)
        {
            return SetAsyncValidator(async () =>
            {
                var error = await condition(validValidation.GetFieldValue());
                
                if (error is not null)
                {
                    validValidation.Context.ValidationOperation =
                        validValidation.Context.ValidationOperationsCollected[0];
                    validValidation.WithError(error);
                    validValidation.Context.ValidationOperation = new ValidationOperation<TField>();
                }

                return new ErrorPredicateValidator(() => error);
            });
        }

        return this;
    }

    public abstract TField Instance();
    
    public abstract TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance);

    public IValidation<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer)
    {
        if (this is IValidValidation<TField> validValidation && validValidation.Context.ValidationOperation!.ExecuteNextValidator)
        {
            var getFieldValueTransformed = () => transformer(validValidation.GetFieldValue());
            return IValidValidation<TFieldTransformed>.CreateFromField(
                getFieldValueTransformed,
                validValidation.CrossErrorToException,
                generalizeError: validValidation.Context.GeneralizeError,
                fieldFullPath: validValidation.Context.FieldName,
                context: validValidation.Context,
                error: validValidation.Context.ValidationOperation.Error,
                message: validValidation.Context.ValidationOperation.Message,
                code: validValidation.Context.ValidationOperation.Code,
                details: validValidation.Context.ValidationOperation.Details,
                httpStatusCode: validValidation.Context.ValidationOperation.HttpStatusCode,
                fieldDisplayName: validValidation.Context.ValidationOperation.FieldDisplayName);
        }

        return IInvalidValidation<TFieldTransformed>.Create();
    }

    public IValidation<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator)
    {
        if (this is IValidValidation<TField> validValidation)
        {
            // TODO: validValidation.Clean(); // Ignore customizations for model validators

            if (validValidation.Context.ValidationOperation!.ExecuteNextValidator)
            {
                // validValidation.Clean(); // Ignore customizations for model validators
                
                var oldContext = validValidation.Context;
                var childContext = validValidation.Context.CloneForChildModelValidator(validValidation.FieldFullPath);
                validator.Context = childContext;
                var childModel = (TChildModel)(object)validValidation.GetFieldValue()!;
                validator.Validate(childModel);
                var newErrors = validator.Context.ErrorsCollected;
                validator.Context = oldContext;
                validator.Context.ErrorsCollected = newErrors;
            }
        }

        return this;
    }

    public async Task RunAsync()
    {
        if (this is not IValidValidation<TField> validValidation)
        {
            return;
        }
        
        await validValidation.Context.ExecuteOperationsCollectedAsync<TField>(useAsync: true);
    }
}