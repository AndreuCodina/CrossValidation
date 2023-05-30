using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record EmptyCollectionValidator<TField>(IEnumerable<TField> FieldValue) : Validator
{
    public override bool IsValid()
    {
        return !FieldValue.Any();
    }

    public override ICrossError CreateError()
    {
        return new CommonCrossError.EmptyCollection();
    }
}