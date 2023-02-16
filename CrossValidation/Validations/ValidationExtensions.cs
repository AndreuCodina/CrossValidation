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
        var ruleToReturn = validation;

        if (validation is IValidValidation<TField?> validRule)
        {
            ruleToReturn = validRule.SetValidator(new NotNullValidator<TField?>(validRule.GetFieldValue()));
        }

        return ruleToReturn.Transform(x => x!);
    }

    public static IValidation<TField> NotNull<TField>(
        this IValidation<TField?> validation)
        where TField : struct
    {
        var ruleToReturn = validation;

        if (validation is IValidValidation<TField?> validRule)
        {
            ruleToReturn = validRule.SetValidator(new NotNullValidator<TField?>(validRule.GetFieldValue()));
        }

        return ruleToReturn.Transform(x => x!.Value);
    }

    public static IValidation<TField?> WhenNotNull<TField, TReturnedField>(
        this IValidation<TField?> validation,
        Func<IValidation<TField>, IValidation<TReturnedField>> notNullRule)
        where TField : struct
    {
        if (validation is not IValidValidation<TField?> validRule)
        {
            return validation;
        }

        if (validRule.GetFieldValue() is not null)
        {
            var ruleReturned = notNullRule(validation.Transform(x => x!.Value));

            if (ruleReturned is IInvalidValidation<TReturnedField>)
            {
                return IInvalidValidation<TField?>.Create();
            }
        }

        return validation;
    }

    public static IValidation<TField?> WhenNotNull<TField, TReturnedField>(
        this IValidation<TField?> validation,
        Func<IValidation<TField>, IValidation<TReturnedField>> notNullRule)
        where TField : class
    {
        if (validation is not IValidValidation<TField?> validRule)
        {
            return validation;
        }

        if (validRule.GetFieldValue() is not null)
        {
            var ruleReturned = notNullRule(validation.Transform(x => x!));

            if (ruleReturned is IInvalidValidation<TReturnedField>)
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
        if (validation is IValidValidation<TField?> validRule)
        {
            return validation.SetValidator(new NullValidator<TField?>(validRule.GetFieldValue()));
        }

        return validation;
    }

    public static IValidation<TField?> Null<TField>(
        this IValidation<TField?> validation)
        where TField : struct
    {
        if (validation is IValidValidation<TField?> validRule)
        {
            return validation.SetValidator(new NullValidator<TField?>(validRule.GetFieldValue()));
        }

        return validation;
    }

    public static IValidation<TField> GreaterThan<TField>(
        this IValidation<TField> validation,
        TField valueToCompare)
        where TField : IComparisonOperators<TField, TField, bool>
    {
        if (validation is IValidValidation<TField> validRule)
        {
            return validation.SetValidator(new GreaterThanValidator<TField>(validRule.GetFieldValue(), valueToCompare));
        }

        return validation;
    }

    public static IValidation<TField> Enum<TField>(
        this IValidation<TField> validation)
        where TField : Enum
    {
        if (validation is IValidValidation<TField> validRule)
        {
            return validation.SetValidator(new EnumValidator<TField>(validRule.GetFieldValue(), typeof(TField)));
        }

        return validation;
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<int> validation)
        where TEnum : Enum
    {
        var ruleToReturn = validation;
        
        if (validation is IValidValidation<int> validRule)
        {
            ruleToReturn = validRule.SetValidator(new EnumValidator<int>(validRule.GetFieldValue(), typeof(TEnum)));
        }
        
        return ruleToReturn.Transform(x => (TEnum)(object)x);
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<string> validation)
        where TEnum : Enum
    {
        var ruleToReturn = validation;
        
        if (validation is IValidValidation<string> validRule)
        {
            ruleToReturn = validRule.SetValidator(
                new EnumValidator<string>(validRule.GetFieldValue(), typeof(TEnum)));
        }

        return ruleToReturn.Transform(x =>
            (TEnum)System.Enum.Parse(typeof(TEnum), x, ignoreCase: true));
    }

    public static IValidation<TField> Enum<TField>(
        this IValidation<TField> validation,
        params TField[] allowedValues)
        where TField : Enum
    {
        if (validation is IValidValidation<TField> validRule)
        {
            return validation.SetValidator(
                new EnumRangeValidator<TField, TField>(validRule.GetFieldValue(), allowedValues));
        }

        return validation;
    }
    
    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<int> validation,
        params TEnum[] allowedValues)
        where TEnum : Enum
    {
        var ruleToReturn = validation;
        
        if (validation is IValidValidation<int> validRule)
        {
            ruleToReturn = validRule.SetValidator(
                new EnumRangeValidator<int, TEnum>(validRule.GetFieldValue(), allowedValues));
        }
        
        return ruleToReturn.Transform(x => (TEnum)(object)x);
    }
    
    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<string> validation,
        params TEnum[] allowedValues)
        where TEnum : Enum
    {
        var ruleToReturn = validation;
        
        if (validation is IValidValidation<string> validRule)
        {
            ruleToReturn = validRule.SetValidator(
                new EnumRangeValidator<string, TEnum>(validRule.GetFieldValue(), allowedValues));
        }
        
        return ruleToReturn.Transform(x =>
            (TEnum)System.Enum.Parse(typeof(TEnum), x, ignoreCase: true));
    }

    public static IValidation<string> LengthRange(
        this IValidation<string> validation,
        int minimum,
        int maximum)
    {
        if (validation is IValidValidation<string> validRule)
        {
            return validation.SetValidator(new LengthRangeValidator(validRule.GetFieldValue(), minimum, maximum));
        }

        return validation;
    }

    public static IValidation<string> MinimumLength(
        this IValidation<string> validation,
        int minimum)
    {
        if (validation is IValidValidation<string> validRule)
        {
            return validation.SetValidator(new MinimumLengthValidator(validRule.GetFieldValue(), minimum));
        }

        return validation;
    }

    public static IValidation<IEnumerable<TInnerType>> ForEach<TInnerType, TReturnedField>(
        this IValidation<IEnumerable<TInnerType>> validation,
        Func<IValidation<TInnerType>, IValidation<TReturnedField>> action)
    {
        if (validation is IValidValidation<IEnumerable<TInnerType>> validRule)
        {
            var fieldCollection = validRule.GetFieldValue();
            var fieldFullPath = validRule.Context.FieldName;
            var index = 0;

            foreach (var innerField in fieldCollection)
            {
                var newRule = IValidValidation<TInnerType>.CreateFromField(
                    innerField,
                    fieldFullPath,
                    validRule.Context,
                    index,
                    validRule.Context.ParentPath);
                var ruleReturned = action(newRule);

                if (ruleReturned is IInvalidValidation<TReturnedField>)
                {
                    return IInvalidValidation<IEnumerable<TInnerType>>.Create();
                }

                index++;
            }
        }

        return validation;
    }
}