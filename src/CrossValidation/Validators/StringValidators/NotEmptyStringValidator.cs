using CrossValidation.Exceptions;

namespace CrossValidation.Validators.StringValidators;

public class NotEmptyStringValidator(string fieldValue) : Validator
{
    public override bool IsValid()
    {
        return fieldValue != "";
    }

    public override BusinessException CreateException()
    {
        return new CommonException.NotEmptyStringException();
    }
}