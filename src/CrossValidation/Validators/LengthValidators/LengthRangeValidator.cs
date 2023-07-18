using CrossValidation.Errors;

namespace CrossValidation.Validators.LengthValidators;

public class LengthRangeValidator(string FieldValue, int Minimum, int Maximum) : LengthValidatorBase
{
    public override bool IsValid()
    {
        return FieldValue.Length >= Minimum
               && FieldValue.Length <= Maximum;
    }

    public override LengthException CreateError()
    {
        return new CommonCrossError.LengthRange(Minimum, Maximum, GetTotalLength(FieldValue));
    }
}