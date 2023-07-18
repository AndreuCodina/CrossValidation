using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public interface IValidator<out TValidationException>
{
    public bool IsValid();
    public TValidationException CreateError();
    public TValidationException? GetError();
}

public abstract class Validator : Validator<BusinessException>;

public abstract class Validator<TValidationException> : IValidator<TValidationException>
    where TValidationException : BusinessException
{
    public abstract bool IsValid();
    
    public abstract TValidationException CreateError();
    
    public TValidationException? GetError()
    {
        return !IsValid() ? (TValidationException?)CreateError() : null;
    }
}