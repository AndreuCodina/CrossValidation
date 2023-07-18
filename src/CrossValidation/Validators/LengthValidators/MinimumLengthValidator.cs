using CrossValidation.Errors;

namespace CrossValidation.Validators.LengthValidators;

public class MinimumLengthValidator(string fieldValue, int minimum) : LengthValidatorBase
{
    public override bool IsValid()
    {
        return fieldValue.Length >= minimum;
    }

    public override LengthException CreateError()
    {
        return new CommonCrossException.MinimumLength(minimum, GetTotalLength(fieldValue));
    }
}