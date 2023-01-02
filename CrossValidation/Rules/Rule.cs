using System.Diagnostics;
using System.Linq.Expressions;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Results;
using CrossValidation.Utils;
using CrossValidation.ValidationContexts;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public class Rule<TField> : IRule<TField>
{
    public RuleState State { get; set; }
    public TField FieldValue { get; set; }
    public ValidationContext Context { get; set; }
    public string FieldFullPath { get; set; }

    private Rule(
        TField fieldValue,
        string? fieldFullPath = null,
        ValidationContext? context = null,
        int? index = null,
        string? parentPath = null)
    {
        State = RuleState.Valid;
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
        return new Rule<TField>(fieldValue, fieldFullPath, context, index, parentPath);
    }

    public static IRule<TField> CreateFromFieldSelector<TModel>(
        TModel model,
        Expression<Func<TModel, TField>> fieldSelector,
        ValidationContext? context = null)
    {
        var fieldInformation = FieldInformationExtractor<TField>
            .Extract(model, fieldSelector);
        return new Rule<TField>(fieldInformation.Value, fieldInformation.SelectionFullPath, context);
    }

    public TField GetFieldValue()
    {
        return FieldValue;
    }

    public bool CanContinueExecutingRule()
    {
        return State is RuleState.Valid
               && Context.ExecuteNextValidator;
    }
    
    public IRule<TField> SetValidator(Func<Validator> validator)
    {
        if (CanContinueExecutingRule())
        {
            var error = validator().GetError();

            if (error is not null)
            {
                FinishWithError(error);
                State = RuleState.Invalid;
            }
        }

        Context.Clean();
        return this;
    }
    

    protected void HandleError(CrossValidationError error)
    {
        Context.AddError(error);
        
        if (Context is {ValidationMode: ValidationMode.StopValidationOnFirstError, Errors: { }})
        {
            throw new CrossValidationException(Context.Errors!);
        }
    }

    public IRule<TField> WithMessage(string message)
    {
        if (CanContinueExecutingRule())
        {
            Context.SetMessage(message);
        }

        return this;
    }

    public IRule<TField> WithCode(string code)
    {
        if (CanContinueExecutingRule())
        {
            Context.SetCode(code);
        }

        return this;
    }
    
    public IRule<TField> WithError(CrossValidationError error)
    {
        if (CanContinueExecutingRule())
        {
            Context.SetError(error);
        }

        return this;
    }
    
    public IRule<TField> WithFieldDisplayName(string fieldDisplayName)
    {
        if (CanContinueExecutingRule())
        {
            Context.SetFieldDisplayName(fieldDisplayName);
        }

        return this;
    }
    
    public IRule<TField> When(bool condition)
    {
        if (CanContinueExecutingRule())
        {
            Context.ExecuteNextValidator = condition;
        }
        
        return this;
    }
    
    public IRule<TField> When(Func<TField, bool> condition)
    {
        if (CanContinueExecutingRule())
        {
            Context.ExecuteNextValidator = condition(FieldValue!);
        }
        
        return this;
    }
    
    public IRule<TField> Must(Func<TField, bool> condition)
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
            throw new UnreachableException();
        }
    }
    
    public IRule<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer)
    {
        var fieldValueTransformed = transformer(FieldValue);
        return Rule<TFieldTransformed>.CreateFromField(fieldValueTransformed, Context.FieldName, Context);
    }
    
    public IRule<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator)
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
    
    private void FinishWithError(CrossValidationError error)
    {
        FillErrorWithCustomizations(error);
        HandleError(error);
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