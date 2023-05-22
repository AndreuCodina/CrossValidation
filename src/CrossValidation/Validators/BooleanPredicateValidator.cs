using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record BooleanPredicateValidator(Func<bool> Predicate) : Validator
{
    public override bool IsValid()
    {
        return Predicate();
    }

    public override CrossError CreateError()
    {
        return new CommonCrossError.Predicate();
    }
}