using CrossValidation.Results;

namespace CrossValidation.Validators;

public abstract record LengthValidator : Validator<ILengthError>
{
    public int GetTotalLength(string value)
    {
        return value.Length;
    }
}