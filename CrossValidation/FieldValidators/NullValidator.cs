using CrossValidation.Results;

namespace CrossValidation.FieldValidators;

public record NullValidator<TField>(TField? FieldValue) : FieldValidator
{
    public override bool IsValid()
    {
        return FieldValue is null;
    }

    public override ValidationError CreateError()
    {
        return new CommonValidationError.Null();
    }
}