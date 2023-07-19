using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class EmptyStringValidator(string fieldValue) : Validator
{
    public override bool IsValid()
    {
        return fieldValue == "";
    }

    public override BusinessException CreateError()
    {
        return new CommonCrossException.EmptyString();
    }
}