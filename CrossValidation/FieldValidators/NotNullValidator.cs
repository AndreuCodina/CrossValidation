using CrossValidation.Results;

namespace CrossValidation.FieldValidators;

public record NotNullValidator<TField>(TField? FieldValue) : FieldValidator
{
    public override bool IsValid()
    {
        return FieldValue is not null;
    }

    public override ValidationError CreateError()
    {
        return new CommonValidationError.NotNull();
    }
}