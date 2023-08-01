using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class NotNullValidator<TField>(TField? fieldValue) : Validator
{
    public override bool IsValid()
    {
        return fieldValue is not null;
    }

    public override BusinessException CreateException()
    {
        return new CommonException.NotNullException();
    }
}