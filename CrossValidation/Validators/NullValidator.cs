using CrossValidation.Results;

namespace CrossValidation.Validators;

public record NullValidator<TField>(TField? FieldValue) : Validator
{
    public override bool IsValid()
    {
        return FieldValue is null;
    }

    public override CrossValidationError CreateError()
    {
        return new CommonCrossValidationError.Null();
    }
}