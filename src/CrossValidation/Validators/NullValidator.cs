using CrossValidation.Errors;
using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class NullValidator<TField>(TField? fieldValue) : Validator
{
    public override bool IsValid()
    {
        return fieldValue is null;
    }

    public override BusinessException CreateError()
    {
        return new CommonCrossError.Null();
    }
}