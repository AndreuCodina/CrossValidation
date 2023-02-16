using System.Diagnostics.Contracts;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Validators;

namespace CrossValidation.Validations;

public interface IValidation<out TField>
{
    [Pure]
    IValidation<TField> SetValidator(IValidator<ICrossError> validator);

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

    [Pure]
    TField Instance();
    
    [Pure]
    TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance);

    IValidation<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator);
}

internal abstract class Validation<TField> : IValidation<TField>
{
    public IValidation<TField> SetValidator(IValidator<ICrossError> validator)
    {
        if (this is IValidValidation<TField> validRule)
        {
            if (validRule.Context.ExecuteNextValidator)
            {
                var error = validator.GetError();

                if (error is not null)
                {
                    validRule.HandleError(error);
                    validRule.Context.Clean();
                    return IInvalidValidation<TField>.Create();
                }
            }

            validRule.Context.Clean();
        }

        return this;
    }

    public IValidation<TField> WithCode(string code)
    {
        if (this is IValidValidation<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.Code = code;
        }

        return this;
    }
    
    public IValidation<TField> WithMessage(string message)
    {
        if (this is IValidValidation<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.Message = message;
        }

        return this;
    }
    
    public IValidation<TField> WithDetails(string details)
    {
        if (this is IValidValidation<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.Details = details;
        }

        return this;
    }

    public IValidation<TField> WithError(ICrossError error)
    {
        if (this is IValidValidation<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.TakeErrorCustomizations(error, overrideContextCustomizations: true);
            validRule.Context.Error = error;
        }

        return this;
    }

    public IValidation<TField> WithFieldDisplayName(string fieldDisplayName)
    {
        if (this is IValidValidation<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.FieldDisplayName = fieldDisplayName;
        }

        return this;
    }

    public IValidation<TField> WithHttpStatusCode(HttpStatusCode code)
    {
        if (this is IValidValidation<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.HttpStatusCode = code;
        }

        return this;
    }

    public IValidation<TField> When(bool condition)
    {
        if (this is IValidValidation<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.ExecuteNextValidator = condition;
        }

        return this;
    }

    public IValidation<TField> When(Func<TField, bool> condition)
    {
        if (this is IValidValidation<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.ExecuteNextValidator = condition(validRule.GetFieldValue());
        }

        return this;
    }

    public IValidation<TField> WhenAsync(Func<TField, Task<bool>> condition)
    {
        if (this is IValidValidation<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.ExecuteNextValidator = condition(validRule.GetFieldValue())
                .GetAwaiter()
                .GetResult();
        }

        return this;
    }

    public IValidation<TField> Must(Func<TField, bool> condition)
    {
        if (this is IValidValidation<TField> validRule)
        {
            return SetValidator(new PredicateValidator(condition(validRule.GetFieldValue())));
        }

        return this;
    }

    public IValidation<TField> MustAsync(Func<TField, Task<bool>> condition)
    {
        if (this is IValidValidation<TField> validRule)
        {
            return SetValidator(new PredicateValidator(
                condition(validRule.GetFieldValue())
                    .GetAwaiter()
                    .GetResult()));
        }

        return this;
    }

    public abstract TField Instance();
    
    public abstract TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance);

    public IValidation<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer)
    {
        if (this is IValidValidation<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            var fieldValueTransformed = transformer(validRule.GetFieldValue());
            return IValidValidation<TFieldTransformed>.CreateFromField(
                fieldValueTransformed,
                validRule.Context.FieldName,
                validRule.Context);
        }

        return IInvalidValidation<TFieldTransformed>.Create();
    }

    public IValidation<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator)
    {
        if (this is IValidValidation<TField> validRule)
        {
            validRule.Context.Clean(); // Ignore customizations for model validators

            if (validRule.Context.ExecuteNextValidator)
            {
                var oldContext = validRule.Context;
                var childContext = validRule.Context.CloneForChildModelValidator(validRule.FieldFullPath);
                validator.Context = childContext;
                var childModel = (TChildModel)(object)validRule.GetFieldValue()!;
                validator.Validate(childModel);
                var newErrors = validator.Context.ErrorsCollected;
                validator.Context = oldContext;
                validator.Context.ErrorsCollected = newErrors;
            }
        }

        return this;
    }
}