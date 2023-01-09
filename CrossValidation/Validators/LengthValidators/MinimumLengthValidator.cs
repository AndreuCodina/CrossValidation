using CrossValidation.Results;

namespace CrossValidation.Validators.LengthValidators;

public record MinimumLengthValidator(string FieldValue, int Minimum) : LengthValidatorBase
{
    public override bool IsValid()
    {
        return FieldValue.Length >= Minimum;
    }

    public override ILengthError CreateError()
    {
        return new CommonValidationError.MinimumLength(Minimum, GetTotalLength(FieldValue));
    }
}