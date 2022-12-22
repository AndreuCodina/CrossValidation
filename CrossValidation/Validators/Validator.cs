using CrossValidation.Results;

namespace CrossValidation.Validators;

public abstract record Validator
{
    public abstract bool IsValid();
    public abstract CrossValidationError CreateError();

    public CrossValidationError? GetError()
    {
        return !IsValid() ? CreateError() : null;
    }
}