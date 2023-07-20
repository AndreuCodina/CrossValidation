using CrossValidation.Exceptions;

namespace CrossValidation.Validators.LengthValidators;

public class LengthRangeValidator(string FieldValue, int Minimum, int Maximum) : LengthValidatorBase
{
    public override bool IsValid()
    {
        return FieldValue.Length >= Minimum
               && FieldValue.Length <= Maximum;
    }

    public override LengthException CreateException()
    {
        return new CommonException.LengthRange(Minimum, Maximum, GetTotalLength(FieldValue));
    }
}