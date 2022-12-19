using CrossValidation.Resources;
using CrossValidation.Results;
using CrossValidation.ValidationContexts;
using CrossValidation.Validators;

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

    public TSelf SetValidator(Validator validator)
    {
        if (Context.ExecuteNextValidator)
        {
            var error = validator.GetError();

            if (error is not null)
            {
                var errorFilled = error with
                {
                    FieldName = Context.FieldName,
                    FieldValue = Context.FieldValue,
                    Message = Context.Message is null && error.Message is null && error.Code is not null
                        ? ErrorResource.ResourceManager.GetString(error.Code!)
                        : Context.Message,
                    FieldDisplayName = error.FieldDisplayName ?? Context.FieldName
                };
                validator.SetError(errorFilled);
                HandleError(errorFilled);
            }
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
    
    public TSelf WithError(ValidationError error)
    {
        Context.SetError(error);
        return GetSelf();
    }
}