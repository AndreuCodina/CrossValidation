using CrossValidation.Exceptions;

namespace CrossValidation.Validators.StringValidators;

public class MinimumLengthValidator(string fieldValue, int minimum) : LengthValidatorBase
{
    public override bool IsValid()
    {
        return fieldValue.Length >= minimum;
    }

    public override LengthException CreateException()
    {
        return new CommonException.MinimumLengthException(minimum, GetTotalLength(fieldValue));
    }
}