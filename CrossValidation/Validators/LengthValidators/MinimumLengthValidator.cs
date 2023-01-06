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
        return new CommonCrossValidationError.MinimumLength(Minimum, GetTotalLength(FieldValue));
    }
}