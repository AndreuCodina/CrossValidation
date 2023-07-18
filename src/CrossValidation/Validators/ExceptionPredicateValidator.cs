using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class ExceptionPredicateValidator(Func<BusinessException?> predicate) : Validator
{
    private BusinessException? _error;
    
    public override bool IsValid()
    {
        _error = predicate();
        return _error is null;
    }

    public override BusinessException CreateError()
    {
        return _error!;
    }
}