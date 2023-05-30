using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record NotEmptyStringValidator(string FieldValue) : Validator
{
    public override bool IsValid()
    {
        return FieldValue != "";
    }

    public override ICrossError CreateError()
    {
        return new CommonCrossError.NotEmptyString();
    }
}