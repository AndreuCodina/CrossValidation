using System.Diagnostics;
using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record EnumRangeValidator<TField, TEnum>(
    TField FieldValue,
    TEnum[] allowedValues) :
    Validator
    where TEnum : Enum
{
    public override bool IsValid()
    {
        if (!Enum.IsDefined(typeof(TEnum), FieldValue!))
        {
            return false;
        }

        TEnum fieldValueEnum;

        if (FieldValue is Enum)
        {
            fieldValueEnum = (TEnum)(object)FieldValue!;
        }
        else if (FieldValue is int)
        {
            fieldValueEnum = (TEnum)(object)FieldValue!;
        }
        else if (FieldValue is string)
        {
            fieldValueEnum = (TEnum)Enum.Parse(typeof(TEnum), (string)(object)FieldValue!, ignoreCase: true);
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

    public override CrossError CreateError()
    {
        return new CommonCrossError.EnumRange();
    }
}