using CrossValidation.Errors;

namespace CrossValidation.Validators;

// TODO: Pass 2 lambdas: success and failure ??
public record PredicateValidator(Func<bool> Predicate) : Validator
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