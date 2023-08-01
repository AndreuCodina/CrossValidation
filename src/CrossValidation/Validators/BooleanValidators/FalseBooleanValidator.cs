using CrossValidation.Exceptions;

namespace CrossValidation.Validators.BooleanValidators;

public class FalseBooleanValidator(bool fieldValue) : Validator
{
    public override bool IsValid()
    {
        return !fieldValue;
    }

    public override BusinessException CreateException()
    {
        return new CommonException.FalseBooleanException();
    }
}