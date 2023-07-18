using CrossValidation.Errors;
using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class BooleanPredicateValidator(Func<bool> predicate) : Validator
{
    public override bool IsValid()
    {
        return predicate();
    }

    public override BusinessException CreateError()
    {
        return new CommonCrossException.Predicate();
    }
}