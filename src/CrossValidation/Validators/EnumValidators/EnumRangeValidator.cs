using System.Diagnostics;
using System.Runtime.CompilerServices;
using CrossValidation.Exceptions;

namespace CrossValidation.Validators.EnumValidators;

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

        TEnum fieldValueEnumCase;

        if (fieldValue is Enum fieldValueEnum)
        {
            fieldValueEnumCase = (TEnum)fieldValueEnum;
        }
        else if (fieldValue is int fieldValueInt)
        {
            fieldValueEnumCase = Unsafe.As<int, TEnum>(ref fieldValueInt);
        }
        else if (fieldValue is string fieldValueString)
        {
            fieldValueEnumCase = (TEnum)Enum.Parse(typeof(TEnum), fieldValueString, ignoreCase: true);
        }
        else
        {
            throw new UnreachableException();
        }
        
        foreach (var allowedValue in allowedValues)
        {
            if (allowedValue.CompareTo(fieldValueEnumCase) == 0)
            {
                return true;
            }
        }
        
        return false;
    }

    public override BusinessException CreateException()
    {
        return new CommonException.EnumRangeException();
    }
}