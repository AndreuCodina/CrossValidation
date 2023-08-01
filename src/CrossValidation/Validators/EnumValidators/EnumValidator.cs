using CrossValidation.Exceptions;

namespace CrossValidation.Validators.EnumValidators;

public class EnumValidator<TField>(
    TField fieldValue,
    Type enumType) : Validator
{
    public override bool IsValid()
    {
        return Enum.IsDefined(enumType, fieldValue!);
    }

    public override BusinessException CreateException()
    {
        return new CommonException.EnumException();
    }
}