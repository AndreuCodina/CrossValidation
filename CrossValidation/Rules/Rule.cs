using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Utils;
using CrossValidation.ValidationContexts;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public interface IRule<out TField>
{
    [Pure]
    IRule<TField> SetValidator(IValidator<ICrossValidationError> validator);

    [Pure]
    IRule<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer);

    [Pure]
    IRule<TField> WithMessage(string message);

    [Pure]
    IRule<TField> WithCode(string code);

    [Pure]
    IRule<TField> WithError(ICrossValidationError error);

    [Pure]
    IRule<TField> WithFieldDisplayName(string fieldDisplayName);

    [Pure]
    IRule<TField> When(bool condition);

    [Pure]
    IRule<TField> When(Func<TField, bool> condition);

    [Pure]
    IRule<TField> WhenAsync(Func<TField, Task<bool>> condition);

    IRule<TField> Must(Func<TField, bool> condition);
    IRule<TField> MustAsync(Func<TField, Task<bool>> condition);

    [Pure]
    TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance);

    IRule<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator);
}

public abstract class Rule<TField> : IRule<TField>
{
    public IRule<TField> SetValidator(IValidator<ICrossValidationError> validator)
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
                    return new InvalidRule<TField>();
                }
            }

            validRule.Context.Clean();
        }

        return this;
    }

    public IRule<TField> WithMessage(string message)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.SetMessage(message);
        }

        return this;
    }

    public IRule<TField> WithCode(string code)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.SetCode(code);
        }

        return this;
    }

    public IRule<TField> WithError(ICrossValidationError error)
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
            validRule.Context.SetFieldDisplayName(fieldDisplayName);
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

    public abstract TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance);

    public IRule<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer)
    {
        if (this is IValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            var fieldValueTransformed = transformer(validRule.GetFieldValue());
            return IValidRule<TFieldTransformed>.CreateFromField(fieldValueTransformed, validRule.Context.FieldName,
                validRule.Context);
        }

        return new InvalidRule<TFieldTransformed>();
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

public interface IValidRule<out TField> : IRule<TField>
{
    public TField GetFieldValue();
    public ValidationContext Context { get; set; }
    public string FieldFullPath { get; set; }
    void HandleError(ICrossValidationError error);
    void TakeErrorCustomizations(ICrossValidationError error, bool overrideContextCustomizations);
    
    public static IRule<TField> CreateFromField(
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null)
    {
        return new ValidRule<TField>(fieldValue, fieldFullPath, context, index, parentPath);
    }

    public static IRule<TField> CreateFromFieldSelector<TModel>(
        TModel model,
        Expression<Func<TModel, TField>> fieldSelector,
        ValidationContext? context = null)
    {
        var fieldInformation = FieldInformationExtractor<TField>
            .Extract(model, fieldSelector);
        return new ValidRule<TField>(fieldInformation.Value, fieldInformation.SelectionFullPath, context);
    }
}

file class ValidRule<TField> :
    Rule<TField>,
    IValidRule<TField>
{
    public TField FieldValue { get; set; }
    public ValidationContext Context { get; set; }
    public string FieldFullPath { get; set; }

    public ValidRule(
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null)
    {
        FieldValue = fieldValue;
        Context = context ?? new ValidationContext();
        Context.FieldValue = fieldValue;
        Context.FieldDisplayName = null;
        FieldFullPath = fieldFullPath ?? "";

        var indexRepresentation = index is not null
            ? $"[{index}]"
            : "";

        var parentPathValue = "";

        if (parentPath is not null)
        {
            parentPathValue = parentPath;
        }
        else if (Context.ParentPath is not null)
        {
            parentPathValue = Context.ParentPath;
        }

        if (parentPathValue is not "")
        {
            parentPathValue += ".";
        }

        Context.FieldName = parentPathValue + fieldFullPath + indexRepresentation;
    }

    public TField GetFieldValue()
    {
        return FieldValue;
    }
    
    public void TakeErrorCustomizations(ICrossValidationError error, bool overrideContextCustomizations)
    {
        if (overrideContextCustomizations)
        {
            if (error.Code is not null)
            {
                Context.Code = error.Code;
            }

            if (error.Message is not null)
            {
                Context.Message = error.Message;
            }
        
            // TODO
            // if (error.Details is not null)
            // {
            //     Context.Details = error.Details;
            // }
        }
    }

    public void HandleError(ICrossValidationError error)
    {
        var errorToAdd = Context.Error ?? error;
        Context.AddError(errorToAdd);

        if (Context is {ValidationMode: ValidationMode.StopValidationOnFirstError})
        {
            throw new CrossValidationException(Context.ErrorsCollected!);
        }
    }

    public override TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        try
        {
            return fieldToInstance(FieldValue);
        }
        catch (CrossValidationException e)
        {
            var error = e.Errors[0];
            error.FieldName = null;
            error.FieldDisplayName = null;
            error.FieldValue = null;
            error.PlaceholderValues = null;
            TakeErrorCustomizations(error, overrideContextCustomizations: false);
            HandleError(error);
            throw new UnreachableException();
        }
    }
}

public interface IInvalidRule<out TField> : IRule<TField>
{
    public static IRule<TField> Create()
    {
        return new InvalidRule<TField>();
    }
}

file class InvalidRule<TField> :
    Rule<TField>,
    IInvalidRule<TField>
{
    public override TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        throw new InvalidOperationException(
            $"Accumulate errors and call {nameof(Instance)} with an invalid rule is not allowed");
    }
}