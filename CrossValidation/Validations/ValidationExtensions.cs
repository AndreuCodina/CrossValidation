using System.Diagnostics;
using System.Numerics;
using CrossValidation.Validators;
using CrossValidation.Validators.LengthValidators;

namespace CrossValidation.Validations;

public static class ValidationExtensions
{
    public static IValidation<TField> NotNull<TField>(
        this IValidation<TField?> validation)
        where TField : class
    {
        var validationToReturn = validation;

        if (validation is IValidValidation<TField?> validValidation)
        {
            validationToReturn = validValidation.SetValidator(new NotNullValidator<TField?>(validValidation.GetFieldValue()));
        }

        return validationToReturn.Transform(x => x!);
    }

    public static IValidation<TField> NotNull<TField>(
        this IValidation<TField?> validation)
        where TField : struct
    {
        var validationToReturn = validation;

        if (validation is IValidValidation<TField?> validValidation)
        {
            validationToReturn = validValidation.SetValidator(new NotNullValidator<TField?>(validValidation.GetFieldValue()));
        }

        return validationToReturn.Transform(x => x!.Value);
    }

    public static IValidation<TField?> WhenNotNull<TField, TReturnedField>(
        this IValidation<TField?> validation,
        Func<IValidation<TField>, IValidation<TReturnedField>> notNullValidation)
        where TField : struct
    {
        if (validation is not IValidValidation<TField?> validValidation)
        {
            return validation;
        }

        if (validValidation.GetFieldValue() is not null)
        {
            var validationReturned = notNullValidation(validation.Transform(x => x!.Value));

            if (validationReturned is IInvalidValidation<TReturnedField>)
            {
                return IInvalidValidation<TField?>.Create();
            }
        }

        return validation;
    }

    public static IValidation<TField?> WhenNotNull<TField, TReturnedField>(
        this IValidation<TField?> validation,
        Func<IValidation<TField>, IValidation<TReturnedField>> notNullValidation)
        where TField : class
    {
        if (validation is not IValidValidation<TField?> validValidation)
        {
            return validation;
        }

        if (validValidation.GetFieldValue() is not null)
        {
            var validationReturned = notNullValidation(validation.Transform(x => x!));

            if (validationReturned is IInvalidValidation<TReturnedField>)
            {
                return IInvalidValidation<TField?>.Create();
            }
        }

        return validation;
    }

    public static IValidation<TField?> Null<TField>(
        this IValidation<TField?> validation)
        where TField : class
    {
        if (validation is IValidValidation<TField?> validValidation)
        {
            return validation.SetValidator(new NullValidator<TField?>(validValidation.GetFieldValue()));
        }

        return validation;
    }

    public static IValidation<TField?> Null<TField>(
        this IValidation<TField?> validation)
        where TField : struct
    {
        if (validation is IValidValidation<TField?> validValidation)
        {
            return validation.SetValidator(new NullValidator<TField?>(validValidation.GetFieldValue()));
        }

        return validation;
    }

    public static IValidation<TField> GreaterThan<TField>(
        this IValidation<TField> validation,
        TField valueToCompare)
        where TField : IComparisonOperators<TField, TField, bool>
    {
        if (validation is IValidValidation<TField> validValidation)
        {
            return validation.SetValidator(new GreaterThanValidator<TField>(validValidation.GetFieldValue(), valueToCompare));
        }

        return validation;
    }

    public static IValidation<TField> Enum<TField>(
        this IValidation<TField> validation)
        where TField : Enum
    {
        if (validation is IValidValidation<TField> validValidation)
        {
            return validation.SetValidator(new EnumValidator<TField>(validValidation.GetFieldValue(), typeof(TField)));
        }

        return validation;
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<int> validation)
        where TEnum : Enum
    {
        var validationToReturn = validation;
        
        if (validation is IValidValidation<int> validValidation)
        {
            validationToReturn = validValidation.SetValidator(new EnumValidator<int>(validValidation.GetFieldValue(), typeof(TEnum)));
        }
        
        return validationToReturn.Transform(x => (TEnum)(object)x);
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<string> validation)
        where TEnum : Enum
    {
        var validationToReturn = validation;
        
        if (validation is IValidValidation<string> validValidation)
        {
            validationToReturn = validValidation.SetValidator(
                new EnumValidator<string>(validValidation.GetFieldValue(), typeof(TEnum)));
        }

        return validationToReturn.Transform(x =>
            (TEnum)System.Enum.Parse(typeof(TEnum), x, ignoreCase: true));
    }

    public static IValidation<TField> Enum<TField>(
        this IValidation<TField> validation,
        params TField[] allowedValues)
        where TField : Enum
    {
        if (validation is IValidValidation<TField> validValidation)
        {
            return validation.SetValidator(
                new EnumRangeValidator<TField, TField>(validValidation.GetFieldValue(), allowedValues));
        }

        return validation;
    }
    
    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<int> validation,
        params TEnum[] allowedValues)
        where TEnum : Enum
    {
        var validationToReturn = validation;
        
        if (validation is IValidValidation<int> validValidation)
        {
            validationToReturn = validValidation.SetValidator(
                new EnumRangeValidator<int, TEnum>(validValidation.GetFieldValue(), allowedValues));
        }
        
        return validationToReturn.Transform(x => (TEnum)(object)x);
    }
    
    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<string> validation,
        params TEnum[] allowedValues)
        where TEnum : Enum
    {
        var validationToReturn = validation;
        
        if (validation is IValidValidation<string> validValidation)
        {
            validationToReturn = validValidation.SetValidator(
                new EnumRangeValidator<string, TEnum>(validValidation.GetFieldValue(), allowedValues));
        }
        
        return validationToReturn.Transform(x =>
            (TEnum)System.Enum.Parse(typeof(TEnum), x, ignoreCase: true));
    }

    public static IValidation<string> LengthRange(
        this IValidation<string> validation,
        int minimum,
        int maximum)
    {
        if (validation is IValidValidation<string> validValidation)
        {
            return validation.SetValidator(new LengthRangeValidator(validValidation.GetFieldValue(), minimum, maximum));
        }

        return validation;
    }

    public static IValidation<string> MinimumLength(
        this IValidation<string> validation,
        int minimum)
    {
        if (validation is IValidValidation<string> validValidation)
        {
            return validation.SetValidator(new MinimumLengthValidator(validValidation.GetFieldValue(), minimum));
        }

        return validation;
    }

    public static IValidation<IEnumerable<TReturnedField>> ForEach<TInnerType, TReturnedField>(
        this IValidation<IEnumerable<TInnerType>> validation,
        Func<IValidation<TInnerType>, IValidation<TReturnedField>> action)
    {
        if (validation is not IValidValidation<IEnumerable<TInnerType>> validValidation)
        {
            return IInvalidValidation<IEnumerable<TReturnedField>>.Create();
        }

        var fieldCollection = validValidation.GetFieldValue();
        var fieldFullPath = validValidation.Context.FieldName;
        var index = 0;
        var areErrors = false;
        var returnedFieldValues = new List<TReturnedField>();

        foreach (var innerField in fieldCollection)
        {
            var newValidation = IValidValidation<TInnerType>.CreateFromField(
                innerField,
                fieldFullPath,
                validValidation.Context,
                index,
                validValidation.Context.ParentPath);
            var validationReturned = action(newValidation);

            if (validationReturned is IInvalidValidation<TReturnedField>)
            {
                if (validValidation.Context.ValidationMode is ValidationMode.StopValidationOnFirstError
                    || validValidation.Context.ValidationMode is ValidationMode.AccumulateFirstErrorEachValidation)
                {
                    return IInvalidValidation<IEnumerable<TReturnedField>>.Create();
                }
                else if (validValidation.Context.ValidationMode is ValidationMode.AccumulateFirstErrorEachValidationAndAllFirstErrorsCollectionIteration)
                {
                    areErrors = true;
                    continue;
                }
                else
                {
                    throw new UnreachableException();
                }
            }
            else if (validationReturned is IValidValidation<TReturnedField> validValidationReturned)
            {
                returnedFieldValues.Add(validValidationReturned.GetFieldValue());
            }
            else
            {
                throw new UnreachableException();
            }

            index++;
        }

        if (areErrors)
        {
            return IInvalidValidation<IEnumerable<TReturnedField>>.Create();
        }
        else
        {
            return IValidValidation<IEnumerable<TReturnedField>>.CreateFromField(
                    returnedFieldValues,
                    fieldFullPath,
                    validValidation.Context,
                    index,
                    validValidation.Context.ParentPath);
        }
    }
}