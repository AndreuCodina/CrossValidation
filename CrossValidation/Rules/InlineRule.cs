using System.Linq.Expressions;
using CrossValidation.Resources;
using CrossValidation.Results;
using CrossValidation.Utils;
using CrossValidation.ValidationContexts;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public class InlineRule<TField>
    : Rule<
        InlineRule<TField>,
        TField,
        InlineValidationContext>
{
    public InlineRule(
        TField? value,
        string? fieldName = null)
    {
        FieldValue = value;
        Context = new InlineValidationContext
        {
            FieldValue = value,
            FieldName = fieldName // TODO: fieldFullPath + indexRepresentation;
        };
    }
    
    public static InlineRule<TField> CreateFromSelector<TModel>(
        TModel model,
        Expression<Func<TModel, TField>> fieldSelector)
    {
        var fieldInformation = Util.GetFieldInformation(fieldSelector, model);
        return new InlineRule<TField>(fieldInformation.Value, fieldInformation.SelectionFullPath);
    }

    protected override InlineRule<TField> GetSelf()
    {
        return this;
    }

    protected override void HandleError(CrossValidationError error)
    {
        Context.AddError(error);
        throw new CrossValidationException(Context.Errors!);
    }

    public InlineRule<TField> When(bool condition)
    {
        Context.ExecuteNextValidator = condition;
        return this;
    }
    
    public InlineRule<TField> When(Func<TField?, bool> condition)
    {
        Context.ExecuteNextValidator = condition(FieldValue);
        return this;
    }
    
    public InlineRule<TField> Must(Func<TField?, bool> condition)
    {
        SetValidator(new PredicateValidator(condition(FieldValue)));
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
}