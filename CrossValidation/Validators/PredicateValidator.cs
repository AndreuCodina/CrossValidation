using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record PredicateValidator(bool Condition) : Validator
{
    public override bool IsValid()
    {
        return Condition;
    }

    public override CrossError CreateError()
    {
        return new CommonCrossError.Predicate();
    }
}