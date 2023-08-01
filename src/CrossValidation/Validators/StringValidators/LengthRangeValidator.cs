using CrossValidation.Exceptions;

namespace CrossValidation.Validators.StringValidators;

public class LengthRangeValidator(string FieldValue, int Minimum, int Maximum) : LengthValidatorBase
{
    public override bool IsValid()
    {
        return FieldValue.Length >= Minimum
               && FieldValue.Length <= Maximum;
    }

    public override LengthException CreateException()
    {
        return new CommonException.LengthRangeException(Minimum, Maximum, GetTotalLength(FieldValue));
    }
}