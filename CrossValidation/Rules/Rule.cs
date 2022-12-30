using System.Linq.Expressions;
using CrossValidation.Resources;
using CrossValidation.Results;
using CrossValidation.Utils;
using CrossValidation.ValidationContexts;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public class Rule<TField>
{
    public bool IsValid { get; set; }
    public TField FieldValue { get; set; }
    public ValidationContext Context { get; set; }
    public string FieldFullPath { get; set; }

    public Rule(
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null)
    {
        IsValid = true;
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

    public static Rule<TField> CreateFromFieldSelector<TModel>(
        TModel model,
        Expression<Func<TModel, TField>> fieldSelector,
        ValidationContext? context = null)
    {
        var fieldInformation = Util.GetFieldInformation(fieldSelector, model);
        return new Rule<TField>(fieldInformation.Value, fieldInformation.SelectionFullPath, context);
    }

    public static Rule<TField> CreateFromField(
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null)
    {
        return new Rule<TField>(fieldValue, fieldFullPath, context, index, parentPath);
    }
    
    public Rule<TField> SetValidator(Func<Validator> validator)
    {
        if (Context.ExecuteNextValidator && IsValid)
        {
            var error = validator().GetError();

            if (error is not null)
            {
                FinishWithError(error);
                IsValid = false;
            }
        }

        Context.Clean();
        return this;
    }


    public void FinishWithError(CrossValidationError error)
    {
        FillErrorWithCustomizations(error);
        HandleError(error);
    }

    protected void HandleError(CrossValidationError error)
    {
        Context.AddError(error);
        
        if (Context is {ValidationMode: ValidationMode.StopOnFirstError, Errors: { }})
        {
            throw new CrossValidationException(Context.Errors!);
        }
    }

    public Rule<TField> WithMessage(string message)
    {
        Context.SetMessage(message);
        return this;
    }

    public Rule<TField> WithCode(string code)
    {
        Context.SetCode(code);
        return this;
    }
    
    public Rule<TField> WithError(CrossValidationError error)
    {
        Context.SetError(error);
        return this;
    }
    
    public Rule<TField> WithFieldDisplayName(string fieldDisplayName)
    {
        Context.SetFieldDisplayName(fieldDisplayName);
        return this;
    }
    
    public Rule<TField> When(bool condition)
    {
        Context.ExecuteNextValidator = condition;
        return this;
    }
    
    public Rule<TField> When(Func<TField, bool> condition)
    {
        Context.ExecuteNextValidator = condition(FieldValue!);
        return this;
    }
    
    public Rule<TField> Must(Func<TField, bool> condition)
    {
        SetValidator(() => new PredicateValidator(condition(FieldValue!)));
        return this;
    }

    public TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        try
        {
            return fieldToInstance(FieldValue!);
        }
        catch (CrossValidationException e)
        {
            var error = FromExceptionToContext(e);
            FinishWithError(error);
            throw new InvalidOperationException("Dead code");
        }
    }
    
    public Rule<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer)
    {
        var fieldValueTransformed = transformer(FieldValue);
        return Rule<TFieldTransformed>.CreateFromField(fieldValueTransformed, Context.FieldName);
    }
    
    public Rule<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator)
    {
        Context.Clean(); // Ignore customizations for model validators
        var oldContext = Context;
        var childContext = Context.CloneForChildModelValidator(FieldFullPath);
        validator.Context = childContext;
        var childModel = (TChildModel)(object)FieldValue!;
        validator.Validate(childModel);
        var newErrors = validator.Context.Errors;
        validator.Context = oldContext;
        validator.Context.Errors = newErrors;
        return this;
    }

    private CrossValidationError FromExceptionToContext(CrossValidationException exception)
    {
        var error = exception.Errors[0];

        Context.Message = GetMessageFromException(error);
        Context.Code ??= error.Code;
        error.PlaceholderValues!.Clear();
        Context.Error ??= error;
        return error;
    }

    private string? GetMessageFromException(CrossValidationError error)
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

    private void FillErrorWithCustomizations(CrossValidationError error)
    {
        error.FieldName = Context.FieldName;
        error.FieldValue = Context.FieldValue;
        error.Message = GetMessageToFill(error);
        error.FieldDisplayName = GetFieldDisplayNameToFill(error);
    }

    private string? GetMessageToFill(CrossValidationError error)
    {
        return Context.Message is null && error.Message is null && error.Code is not null
            ? ErrorResource.ResourceManager.GetString(error.Code)
            : Context.Message;
    }
    
    private string GetFieldDisplayNameToFill(CrossValidationError error)
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
}