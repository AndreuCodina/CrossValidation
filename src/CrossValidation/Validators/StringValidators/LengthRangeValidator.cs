using CrossValidation.Exceptions;

namespace CrossValidation.Validators.StringValidators;

public class LengthRangeValidator(string fieldValue, int minimum, int maximum) : Validator
{
    public override bool IsValid()
    {
        if (minimum < 0)
        {
            throw new ArgumentException("The minimum length cannot be less than zero");
        }
        else if (maximum < 0)
        {
            throw new ArgumentException("The maximum length cannot be less than zero");
        }
        else if (minimum > maximum)
        {
            throw new ArgumentException("The minimum length cannot be greater than the maximum length");
        }

        return fieldValue.Length >= minimum
               && fieldValue.Length <= maximum;
    }

    public override BusinessException CreateException()
    {
        return new CommonException.LengthRangeException(minimum, maximum, fieldValue.Length);
    }
}