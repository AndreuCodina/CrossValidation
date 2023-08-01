using CrossValidation.Exceptions;

namespace CrossValidation.Validators.PredicateValidators;

public class BooleanPredicateValidator(Func<bool> predicate) : Validator
{
    public override bool IsValid()
    {
        return predicate();
    }

    public override BusinessException CreateException()
    {
        return new CommonException.PredicateException();
    }
}