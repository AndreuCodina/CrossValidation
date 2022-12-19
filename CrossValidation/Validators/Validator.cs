using CrossValidation.Results;

namespace CrossValidation.Validators;

public abstract record Validator
{
    private ValidationError? _error;
    public abstract bool IsValid();
    public abstract ValidationError CreateError();

    public ValidationError? GetError()
    {
        return !IsValid() ? CreateError() : null;
    }
    
    public void SetError(ValidationError error)
    {
        _error = error;
    }
}