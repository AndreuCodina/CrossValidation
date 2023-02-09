using CrossValidation.Errors;

namespace CrossValidation.Validators.LengthValidators;

public record MinimumLengthValidator(string FieldValue, int Minimum) : LengthValidatorBase
{
    public override bool IsValid()
    {
        return FieldValue.Length >= Minimum;
    }

    public override ILengthError CreateError()
    {
        return new CommonCrossError.MinimumLength(Minimum, GetTotalLength(FieldValue));
    }
}