using CrossValidation.Results;

namespace CrossValidation.Validators;

public record PredicateValidator(bool Condition) : Validator
{
    public override bool IsValid()
    {
        return Condition;
    }

    public override CrossValidationError CreateError()
    {
        return new CommonCrossValidationError.Predicate();
    }
}