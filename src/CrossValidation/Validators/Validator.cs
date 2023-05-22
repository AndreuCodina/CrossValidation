using CrossValidation.Errors;

namespace CrossValidation.Validators;

public interface IValidator<out TValidationError>
{
    public bool IsValid();
    public TValidationError CreateError();
    public TValidationError? GetError();
}

public abstract record Validator : Validator<ICrossError>;

public abstract record Validator<TValidationError> : IValidator<TValidationError>
    where TValidationError : class, ICrossError
{
    public abstract bool IsValid();
    
    public abstract TValidationError CreateError();
    
    public TValidationError? GetError()
    {
        return !IsValid() ? (TValidationError?)CreateError() : null;
    }
}