using CrossValidation.Exceptions;

namespace CrossValidation.Validators.StringValidators;

public class LengthRangeValidator(string fieldValue, int minimum, int maximum) : LengthValidatorBase
{
    public override bool IsValid()
    {
        if (minimum > maximum)
        {
            throw new ArgumentException("The minimum length cannot be greater than the maximum length");
        }
        else if (maximum < minimum)
        {
            throw new ArgumentException("The maximum length cannot be less than the minimum length");
        }

        return fieldValue.Length >= minimum
               && fieldValue.Length <= maximum;
    }

    public override LengthException CreateException()
    {
        return new CommonException.LengthRangeException(minimum, maximum, fieldValue.Length);
    }
}