using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record PredicateValidator(bool Condition) : Validator
{
    public override bool IsValid()
    {
        return Condition;
    }

    public override ValidationError CreateError()
    {
        return new CommonValidationError.Predicate();
    }
}