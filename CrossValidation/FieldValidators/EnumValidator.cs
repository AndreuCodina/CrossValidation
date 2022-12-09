using CrossValidation.Results;

namespace CrossValidation.FieldValidators;

public record EnumValidator<TField>(
    TField FieldValue,
    Type enumType) : FieldValidator
{
    public override bool IsValid()
    {
        return Enum.IsDefined(enumType, FieldValue);
    }

    public override ValidationError CreateError()
    {
        return new CommonValidationError.Enum();
    }
}