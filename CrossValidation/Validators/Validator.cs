using CrossValidation.Errors;

namespace CrossValidation.Validators;

public interface IValidator<out TValidationError>
{
    public bool IsValid();
    public TValidationError CreateError();
    public TValidationError? GetError();
}

public abstract record Validator : Validator<ValidationError>;

public abstract record Validator<TValidationError> : IValidator<TValidationError>
    where TValidationError : class, IValidationError
{
    public abstract bool IsValid();
    
    public abstract TValidationError CreateError();
    
    public TValidationError? GetError()
    {
        return !IsValid() ? (TValidationError?)CreateError() : null;
    }
}