using CrossValidation.FieldValidators;
using CrossValidation.Results;
using CrossValidation.ValidationContexts;

namespace CrossValidation.Rules;

/// <summary>
/// Rule to validate a field
/// </summary>
public abstract class Rule<TSelf, TField, TValidationContext>
    where TValidationContext : ValidationContext
{
    public TField? FieldValue { get; set; }
    protected TValidationContext Context { get; set; }

    protected abstract TSelf GetSelf();

    public TSelf SetValidator(FieldValidator validator)
    {
        // if (Context.ValidationMode == ValidationMode.StopOnFirstError && Context.HasCurrentModelValidatorAnyError){}
        // Execute validator and take its properties and customize the error to add with ValidationContext.AddError(...)
        // var foo = this;

        if (validator.HasError())
        {
            // TODO:
            // If the model validator has some error && Context.ValidationMode == ValidationMode.StopOnFirstError,
            // add modelValidator.Errors to Context.Errors and return
            
            
            // HandleError / CreateErrorMessage / etc.
            // throw new ValidationException(new List<ValidationError>());
            validator.Error = validator.Error! with
            {
                FieldName = Context.FieldName,
                FieldValue = FieldValue
            };
            HandleError(validator.Error!);
        }

        Context.Clean();
        return GetSelf();
    }

    protected abstract void HandleError(ValidationError error);

    public TSelf WithMessage(string message)
    {
        Context.SetMessage(message);
        return GetSelf();
    }

    public TSelf WithCode(string code)
    {
        Context.SetCode(code);
        return GetSelf();
    }
}