using CrossValidation.Exceptions;

namespace CrossValidation.Validators.StringValidators;

public class MinimumLengthValidator(string fieldValue, int minimum) : Validator
{
    public override bool IsValid()
    {
        return fieldValue.Length >= minimum;
    }

    public override BusinessException CreateException()
    {
        return new CommonException.MinimumLengthException(minimum, fieldValue.Length);
    }
}