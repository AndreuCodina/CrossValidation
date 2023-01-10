using CrossValidation.Errors;

namespace CrossValidation.Validators.LengthValidators;

public abstract record LengthValidatorBase : Validator<ILengthError>
{
    public int GetTotalLength(string value)
    {
        return value.Length;
    }
}