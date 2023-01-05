using CrossValidation.Results;

namespace CrossValidation.Validators;

public record LengthRangeValidator(string FieldValue, int Minimum, int Maximum) : LengthValidator
{
    public override bool IsValid()
    {
        return FieldValue.Length >= Minimum
               && FieldValue.Length <= Maximum;
    }

    public override ILengthError CreateError()
    {
        return new CommonCrossValidationError.LengthRange(Minimum, Maximum, GetTotalLength(FieldValue));
    }
}