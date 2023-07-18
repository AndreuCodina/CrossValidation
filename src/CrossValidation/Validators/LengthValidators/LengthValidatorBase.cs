using CrossValidation.Exceptions;

namespace CrossValidation.Validators.LengthValidators;

public abstract class LengthValidatorBase : Validator<LengthException>
{
    protected int GetTotalLength(string value)
    {
        return value.Length;
    }
}