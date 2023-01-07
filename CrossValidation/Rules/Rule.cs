using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
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
        if (this is ValidRule<TField> validRule)
        {
            if (validRule.Context.ExecuteNextValidator)
            {
                var error = validator.GetError();

                if (error is not null)
                {
                    validRule.FinishWithError(error);
                    return new InvalidRule<TField>();
                }
            }

            validRule.Context.Clean();
        }

        return this;
    }

    public IRule<TField> WithMessage(string message)
    {
        if (this is ValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.SetMessage(message);
        }

        return this;
    }

    public IRule<TField> WithCode(string code)
    {
        if (this is ValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.SetCode(code);
        }

        return this;
    }

    public IRule<TField> WithError(ICrossValidationError error)
    {
        if (this is ValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.SetError(error);
        }

        return this;
    }

    public IRule<TField> WithFieldDisplayName(string fieldDisplayName)
    {
        if (this is ValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.SetFieldDisplayName(fieldDisplayName);
        }

        return this;
    }

    public IRule<TField> When(bool condition)
    {
        if (this is ValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.ExecuteNextValidator = condition;
        }

        return this;
    }

    public IRule<TField> When(Func<TField, bool> condition)
    {
        if (this is ValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.ExecuteNextValidator = condition(validRule.FieldValue);
        }

        return this;
    }

    public IRule<TField> WhenAsync(Func<TField, Task<bool>> condition)
    {
        if (this is ValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            validRule.Context.ExecuteNextValidator = condition(validRule.FieldValue)
                .GetAwaiter()
                .GetResult();
        }

        return this;
    }

    public IRule<TField> Must(Func<TField, bool> condition)
    {
        if (this is ValidRule<TField> validRule)
        {
            SetValidator(new PredicateValidator(condition(validRule.FieldValue)));
        }

        return this;
    }

    public IRule<TField> MustAsync(Func<TField, Task<bool>> condition)
    {
        if (this is ValidRule<TField> validRule)
        {
            return SetValidator(new PredicateValidator(
                condition(validRule.FieldValue)
                    .GetAwaiter()
                    .GetResult()));
        }

        return this;
    }

    public abstract TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance);

    public IRule<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer)
    {
        if (this is ValidRule<TField> validRule && validRule.Context.ExecuteNextValidator)
        {
            var fieldValueTransformed = transformer(validRule.FieldValue);
            return ValidRule<TFieldTransformed>.CreateFromField(fieldValueTransformed, validRule.Context.FieldName,
                validRule.Context);
        }

        return new InvalidRule<TFieldTransformed>();
    }

    public IRule<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator)
    {
        if (this is ValidRule<TField> validRule)
        {
            validRule.Context.Clean(); // Ignore customizations for model validators

            if (validRule.Context.ExecuteNextValidator)
            {
                var oldContext = validRule.Context;
                var childContext = validRule.Context.CloneForChildModelValidator(validRule.FieldFullPath);
                validator.Context = childContext;
                var childModel = (TChildModel)(object)validRule.FieldValue!;
                validator.Validate(childModel);
                var newErrors = validator.Context.Errors;
                validator.Context = oldContext;
                validator.Context.Errors = newErrors;
            }
        }

        return this;
    }
}

public class ValidRule<TField> : Rule<TField>
{
    public TField FieldValue { get; set; }
    public ValidationContext Context { get; set; }
    public string FieldFullPath { get; set; }

    private ValidRule(
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
    
    public void FinishWithError(ICrossValidationError error)
    {
        FillErrorWithCustomizations(error);
        HandleError(error);
    }

    private void HandleError(ICrossValidationError error)
    {
        Context.AddError(error);

        if (Context is {ValidationMode: ValidationMode.StopValidationOnFirstError, Errors: { }})
        {
            throw new CrossValidationException(Context.Errors!);
        }
    }

    private ICrossValidationError FromExceptionToContext(CrossValidationException exception)
    {
        var error = exception.Errors[0];
        Context.Message = GetMessageFromException(error);
        Context.Code ??= error.Code;
        error.PlaceholderValues!.Clear();
        Context.Error ??= error;
        return error;
    }

    private string? GetMessageFromException(ICrossValidationError error)
    {
        if (Context.Message is not null)
        {
            return Context.Message;
        }

        if (Context.Code is not null)
        {
            return ErrorResource.ResourceManager.GetString(Context.Code)!;
        }
        else
        {
            if (error.Message is not null)
            {
                return error.Message;
            }
            else if (error.Code is not null)
            {
                return ErrorResource.ResourceManager.GetString(error.Code)!;
            }
        }

        return null;
    }

    private void FillErrorWithCustomizations(ICrossValidationError error)
    {
        error.FieldName = Context.FieldName;
        error.FieldValue = Context.FieldValue!;
        error.Message = GetMessageToFill(error);
        error.FieldDisplayName = GetFieldDisplayNameToFill(error);
    }

    private string? GetMessageToFill(ICrossValidationError error)
    {
        return Context.Message is null && error.Message is null && error.Code is not null
            ? ErrorResource.ResourceManager.GetString(error.Code)
            : Context.Message;
    }

    private string GetFieldDisplayNameToFill(ICrossValidationError error)
    {
        if (Context.FieldDisplayName is not null)
        {
            return Context.FieldDisplayName;
        }
        else if (error.FieldDisplayName is not null and not "")
        {
            return error.FieldDisplayName;
        }

        return error.FieldName!;
    }

    public override TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        try
        {
            return fieldToInstance(FieldValue);
        }
        catch (CrossValidationException e)
        {
            var error = FromExceptionToContext(e);
            FinishWithError(error);
            throw new UnreachableException();
        }
    }
}

public class InvalidRule<TField> : Rule<TField>
{
    public override TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        throw new InvalidOperationException(
            $"Accumulate errors and call {nameof(Instance)} with an invalid rule is not allowed");
    }
}