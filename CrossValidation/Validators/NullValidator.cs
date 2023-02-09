using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record NullValidator<TField>(TField? FieldValue) : Validator
{
    public override bool IsValid()
    {
        return FieldValue is null;
    }

    public override CrossError CreateError()
    {
        return new CommonCrossError.Null();
    }
}