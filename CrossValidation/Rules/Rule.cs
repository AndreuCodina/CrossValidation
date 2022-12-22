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
                FinishWithError(error);
            }
        }

        Context.Clean();
        return GetSelf();
    }

    public void FinishWithError(CrossValidationError error)
    {
        FillErrorWithCustomizations(error);
        HandleError(error);
    }

    protected abstract void HandleError(CrossValidationError error);

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
    
    public TSelf WithError(CrossValidationError error)
    {
        Context.SetError(error);
        return GetSelf();
    }
    
    public TSelf WithFieldDisplayName(string fieldDisplayName)
    {
        Context.SetFieldDisplayName(fieldDisplayName);
        return GetSelf();
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
        else if (error.FieldDisplayName is not null)
        {
            return error.FieldDisplayName;
        }

        return error.FieldName;
    }
}