using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public abstract class Validator
{
    public abstract bool IsValid();
    
    public abstract BusinessException CreateException();
    
    public BusinessException? GetException()
    {
        return !IsValid() ? (BusinessException?)CreateException() : null;
    }
}