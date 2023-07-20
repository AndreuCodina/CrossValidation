using CrossValidation.Exceptions;

namespace CrossValidation.Validators.LengthValidators;

public class MaximumLengthValidator(string fieldValue, int maximum) : LengthValidatorBase
{
    public override bool IsValid()
    {
        return fieldValue.Length <= maximum;
    }

    public override LengthException CreateException()
    {
        return new CommonException.MaximumLength(maximum, GetTotalLength(fieldValue));
    }
}