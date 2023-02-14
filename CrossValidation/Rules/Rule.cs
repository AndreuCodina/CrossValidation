using System.Diagnostics.Contracts;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public interface IRule<out TField>
{
    [Pure]
    IRule<TField> SetValidator(IValidator<ICrossError> validator);

    [Pure]
    IRule<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer);

    [Pure]
    IRule<TField> WithCode(string code);
    
    [Pure]
    IRule<TField> WithMessage(string message);
    
    [Pure]
    IRule<TField> WithDetails(string details);

    [Pure]
    IRule<TField> WithError(ICrossError error);

    [Pure]
    IRule<TField> WithFieldDisplayName(string fieldDisplayName);
    
    [Pure]
    IRule<TField> WithHttpStatusCode(HttpStatusCode code);

    [Pure]
    IRule<TField> When(bool condition);

    [Pure]
    IRule<TField> When(Func<TField, bool> condition);

    [Pure]
    IRule<TField> WhenAsync(Func<TField, Task<bool>> condition);

    IRule<TField> Must(Func<TField, bool> condition);
    IRule<TField> MustAsync(Func<TField, Task<bool>> condition);

    [Pure]
    TField Instance();
    
    [Pure]
    TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance);

    IRule<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator);
}

internal abstract class Rule<TField> : IRule<TField>
{
    public IRule<TField> SetValidator(IValidator<ICrossError> validator)
    {
        if (this is IValidRule<TField> validRule)
        {
            if (validRule.Context.ExecuteNextValidator)
            {
                var error = validator.GetError();

                if (error is not null)
                {
                    validRule.HandleError(error);
                    validRule.Context.Clean();
                    return IInvalidRule<TField>.Create();
                }
            }

            validRule.Context.Clean();
        }

        return this;
    }

    public IRule<TField> WithCode(string code)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.Code = code;
        }

        return this;
    }
    
    public IRule<TField> WithMessage(string message)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.Message = message;
        }

        return this;
    }
    
    public IRule<TField> WithDetails(string details)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.Details = details;
        }

        return this;
    }

    public IRule<TField> WithError(ICrossError error)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.TakeErrorCustomizations(error, overrideContextCustomizations: true);
            validRule.Context.Error = error;
        }

        return this;
    }

    public IRule<TField> WithFieldDisplayName(string fieldDisplayName)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.FieldDisplayName = fieldDisplayName;
        }

        return this;
    }

    public IRule<TField> WithHttpStatusCode(HttpStatusCode code)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.HttpStatusCode = code;
        }

        return this;
    }

    public IRule<TField> When(bool condition)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.ExecuteNextValidator = condition;
        }

        return this;
    }

    public IRule<TField> When(Func<TField, bool> condition)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.ExecuteNextValidator = condition(validRule.GetFieldValue());
        }

        return this;
    }

    public IRule<TField> WhenAsync(Func<TField, Task<bool>> condition)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.ExecuteNextValidator = condition(validRule.GetFieldValue())
                .GetAwaiter()
                .GetResult();
        }

        return this;
    }

    public IRule<TField> Must(Func<TField, bool> condition)
    {
        if (this is IValidRule<TField> validRule)
        {
            return SetValidator(new PredicateValidator(condition(validRule.GetFieldValue())));
        }

        return this;
    }

    public IRule<TField> MustAsync(Func<TField, Task<bool>> condition)
    {
        if (this is IValidRule<TField> validRule)
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

    public IRule<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            var fieldValueTransformed = transformer(validRule.GetFieldValue());
            return IValidRule<TFieldTransformed>.CreateFromField(
                fieldValueTransformed,
                validRule.Context.FieldName,
                validRule.Context);
        }

        return IInvalidRule<TFieldTransformed>.Create();
    }

    public IRule<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator)
    {
        if (this is IValidRule<TField> validRule)
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