using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public interface IValidator<out TValidationException>
{
    public bool IsValid();
    public TValidationException CreateException();
    public TValidationException? GetException();
}

public abstract class Validator : Validator<BusinessException>;

public abstract class Validator<TValidationException> : IValidator<TValidationException>
    where TValidationException : BusinessException
{
    public abstract bool IsValid();
    
    public abstract TValidationException CreateException();
    
    public TValidationException? GetException()
    {
        return !IsValid() ? (TValidationException?)CreateException() : null;
    }
}