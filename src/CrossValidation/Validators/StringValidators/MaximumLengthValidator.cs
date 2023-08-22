using CrossValidation.Exceptions;

namespace CrossValidation.Validators.StringValidators;

public class MaximumLengthValidator(string fieldValue, int maximum) : Validator
{
    public override bool IsValid()
    {
        return fieldValue.Length <= maximum;
    }

    public override BusinessException CreateException()
    {
        return new CommonException.MaximumLengthException(maximum, fieldValue.Length);
    }
}