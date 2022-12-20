using CrossValidation.Results;

namespace CrossValidation.Validators;

public record LengthRangeValidator(string FieldValue, int Minimum, int Maximum) : Validator
{
    public override bool IsValid()
    {
        return FieldValue.Length >= Minimum
               && FieldValue.Length <= Maximum;
    }

    public override CrossValidationError CreateError()
    {
        return new CommonCrossValidationError.LengthRange(Minimum, Maximum);
    }
}