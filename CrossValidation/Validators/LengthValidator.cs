using CrossValidation.Results;

namespace CrossValidation.Validators;

public record LengthValidator(
    string FieldValue,
    int Minimum,
    int Maximum) : Validator
{
    public override bool IsValid()
    {
        return FieldValue.Length >= Minimum
               && FieldValue.Length <= Maximum;
    }

    public override ValidationError CreateError()
    {
        return new CommonValidationError.Length(Minimum, Maximum);
    }
}