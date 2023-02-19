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
        // var oldContext = new ValidationContext
        // {
        //     Details = validRule.Context.Details,
        //     Error = validRule.Context.Error,
        //     Message = validRule.Context.Message,
        //     Code = validRule.Context.Code,
        //     ExecuteNextValidator = validRule.Context.ExecuteNextValidator,
        //     HttpStatusCode = validRule.Context.HttpStatusCode,
        //     FieldDisplayName = validRule.Context.FieldDisplayName,
        //     ErrorsCollected = validRule.Context.ErrorsCollected,
        //     ValidationMode = validRule.Context.ValidationMode,
        //     ParentPath = validRule.Context.ParentPath,
        //     FieldName = validRule.Context.FieldName,
        //     IsChildContext = validRule.Context.IsChildContext,
        //     FieldValue = validRule.Context.FieldValue
        // };

        foreach (var innerField in fieldCollection)
        {
            var newRule = IValidValidation<TInnerType>.CreateFromField(
                innerField,
                fieldFullPath,
                validValidation.Context,
                index,
                validValidation.Context.ParentPath,
                validValidation);
            var ruleReturned = action(newRule);

            if (ruleReturned is IInvalidValidation<TReturnedField>)
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
            else if (ruleReturned is IValidValidation<TReturnedField> validRuleReturned)
            {
                returnedFieldValues.Add(validRuleReturned.GetFieldValue());
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
                    validValidation.Context.ParentPath,
                    validValidation);
        }
    }
}