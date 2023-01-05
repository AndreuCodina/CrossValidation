using CrossValidation.Errors;

namespace CrossValidation.Validators;

public interface IValidator<out TValidationError>
{
    public bool IsValid();
    public TValidationError CreateError();
    public TValidationError? GetError();
}

public abstract record Validator : Validator<CrossValidationError>;

public abstract record Validator<TValidationError> : IValidator<TValidationError>
    where TValidationError : class, ICrossValidationError
{
    public abstract bool IsValid();
    
    public abstract TValidationError CreateError();
    
    public TValidationError? GetError()
    {
        var error = (TValidationError?)CreateError();
        return !IsValid() ? error : null;
    }
}