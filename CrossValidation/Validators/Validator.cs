using CrossValidation.Results;

namespace CrossValidation.Validators;

public abstract record Validator
{
    private CrossValidationError? _error;
    public abstract bool IsValid();
    public abstract CrossValidationError CreateError();

    public CrossValidationError? GetError()
    {
        return !IsValid() ? CreateError() : null;
    }
    
    public void SetError(CrossValidationError error)
    {
        _error = error;
    }
}