using CrossValidation.Results;

namespace CrossValidation.FieldValidators;

public record NullValidator<TField>(TField? FieldValue) : FieldValidator
{
    protected override bool IsValid()
    {
        return FieldValue is null;
    }

    protected override ValidationError CreateError()
    {
        return new CommonValidationError.Null();
    }
}