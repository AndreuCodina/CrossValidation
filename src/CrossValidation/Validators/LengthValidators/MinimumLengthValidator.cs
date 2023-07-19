using CrossValidation.Exceptions;

namespace CrossValidation.Validators.LengthValidators;

public class MinimumLengthValidator(string fieldValue, int minimum) : LengthValidatorBase
{
    public override bool IsValid()
    {
        return fieldValue.Length >= minimum;
    }

    public override LengthException CreateException()
    {
        return new CommonCrossException.MinimumLength(minimum, GetTotalLength(fieldValue));
    }
}