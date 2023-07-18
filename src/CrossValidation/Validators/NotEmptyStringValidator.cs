using CrossValidation.Errors;
using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class NotEmptyStringValidator(string fieldValue) : Validator
{
    public override bool IsValid()
    {
        return fieldValue != "";
    }

    public override BusinessException CreateError()
    {
        return new CommonCrossException.NotEmptyString();
    }
}