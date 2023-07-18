using System.Diagnostics;
using CrossValidation.Errors;
using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class EnumRangeValidator<TField, TEnum>(
    TField fieldValue,
    TEnum[] allowedValues) :
    Validator
    where TEnum : Enum
{
    public override bool IsValid()
    {
        if (!Enum.IsDefined(typeof(TEnum), fieldValue!))
        {
            return false;
        }

        TEnum fieldValueEnum;

        if (fieldValue is Enum)
        {
            fieldValueEnum = (TEnum)(object)fieldValue!;
        }
        else if (fieldValue is int)
        {
            fieldValueEnum = (TEnum)(object)fieldValue!;
        }
        else if (fieldValue is string)
        {
            fieldValueEnum = (TEnum)Enum.Parse(typeof(TEnum), (string)(object)fieldValue!, ignoreCase: true);
        }
        else
        {
            throw new UnreachableException();
        }
        
        foreach (var allowedValue in allowedValues)
        {
            if (allowedValue.CompareTo(fieldValueEnum) == 0)
            {
                return true;
            }
        }
        
        return false;
    }

    public override BusinessException CreateError()
    {
        return new CommonCrossException.EnumRange();
    }
}