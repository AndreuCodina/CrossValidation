using CrossValidation.Results;

namespace CrossValidation.FieldValidators;

public record NotNullValidator<TField>(TField? FieldValue) : FieldValidator
{
    protected override bool IsValid()
    {
        return FieldValue is not null;
    }

    protected override ValidationError CreateError()
    {
        return new CommonValidationError.NotNull();
    }
}