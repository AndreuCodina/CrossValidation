﻿using System.Numerics;
using CrossValidation.Validators;
using CrossValidation.Validators.LengthValidators;

namespace CrossValidation.Rules;

public static class RuleExtensions
{
    public static IRule<TField> NotNull<TField>(
        this IRule<TField?> rule)
        where TField : class
    {
        var ruleToReturn = rule;
        
        if (rule is ValidRule<TField?> validRule)
        {
            ruleToReturn = validRule.SetValidator(new NotNullValidator<TField?>(validRule.FieldValue));
        }

        return ruleToReturn.Transform(x => x!);
    }

    public static IRule<TField> NotNull<TField>(
        this IRule<TField?> rule)
        where TField : struct
    {
        var ruleToReturn = rule;
        
        if (rule is ValidRule<TField?> validRule)
        {
            ruleToReturn = validRule.SetValidator(new NotNullValidator<TField?>(validRule.FieldValue));
        }

        return ruleToReturn.Transform(x => x!.Value);
    }

    public static IRule<TField?> WhenNotNull<TField, TReturnedField>(
        this IRule<TField?> rule,
        Func<IRule<TField>, IRule<TReturnedField>> notNullRule)
        where TField : struct
    {
        if (rule is ValidRule<TField?> validRule)
        {
            var validator = new NotNullValidator<TField?>(validRule.FieldValue);
            
            if (validator.IsValid())
            {
                var ruleReturned = notNullRule(rule.Transform(x => x!.Value));
                
                if (ruleReturned is InvalidRule<TReturnedField>)
                {
                    return new InvalidRule<TField?>();
                }
                
                // TODO: Remove
                // return ruleReturned.Transform<TField?>(x => x);
            }
        }

        return rule;
    }

    public static IRule<TField?> WhenNotNull<TField>(
        this IRule<TField?> rule,
        Func<IRule<TField>, IRule<TField>> notNullRule)
        where TField : class
    {
        if (rule is ValidRule<TField?> validRule)
        {
            var validator = new NotNullValidator<TField?>(validRule.FieldValue);
            
            if (validator.IsValid())
            {
                var ruleToReturn = notNullRule(rule.Transform(x => x!));
                return ruleToReturn.Transform<TField?>(x => x);
            }
        }

        return rule;
    }

    public static IRule<TField?> Null<TField>(
        this IRule<TField?> rule)
        where TField : class
    {
        if (rule is ValidRule<TField?> validRule)
        {
            return rule.SetValidator(new NullValidator<TField?>(validRule.FieldValue));
        }

        return rule;
    }

    public static IRule<TField?> Null<TField>(
        this IRule<TField?> rule)
        where TField : struct
    {
        if (rule is ValidRule<TField?> validRule)
        {
            return rule.SetValidator(new NullValidator<TField?>(validRule.FieldValue));
        }

        return rule;
    }

    public static IRule<TField> GreaterThan<TField>(
        this IRule<TField> rule,
        TField valueToCompare)
        where TField : IComparisonOperators<TField, TField, bool>
    {
        if (rule is ValidRule<TField> validRule)
        {
            return rule.SetValidator(new GreaterThanValidator<TField>(validRule.FieldValue, valueToCompare));
        }

        return rule;
    }

    public static IRule<TField> IsInEnum<TField>(
        this IRule<TField> rule)
        where TField : Enum
    {
        if (rule is ValidRule<TField> validRule)
        {
            return rule.SetValidator(new EnumValidator<TField>(validRule.FieldValue, typeof(TField)));
        }

        return rule;
    }

    public static IRule<int> IsInEnum<TEnum>(
        this IRule<int> rule)
        where TEnum : Enum
    {
        if (rule is ValidRule<int> validRule)
        {
            return rule.SetValidator(new EnumValidator<int>(validRule.FieldValue, typeof(TEnum)));
        }

        return rule;
    }

    public static IRule<string> IsInEnum<TEnum>(
        this IRule<string> rule)
        where TEnum : Enum
    {
        if (rule is ValidRule<string> validRule)
        {
            return rule.SetValidator(new EnumValidator<string>(validRule.FieldValue, typeof(TEnum)));
        }

        return rule;
    }

    public static IRule<string> LengthRange(
        this IRule<string> rule,
        int minimum,
        int maximum)
    {
        if (rule is ValidRule<string> validRule)
        {
            return rule.SetValidator(new LengthRangeValidator(validRule.FieldValue, minimum, maximum));
        }

        return rule;
    }
    
    public static IRule<string> MinimumLength(
        this IRule<string> rule,
        int minimum)
    {
        if (rule is ValidRule<string> validRule)
        {
            return rule.SetValidator(new MinimumLengthValidator(validRule.FieldValue, minimum));
        }

        return rule;
    }

    public static IRule<IEnumerable<TInnerType>> ForEach<TInnerType, TReturnedField>(
        this IRule<IEnumerable<TInnerType>> rule,
        Func<IRule<TInnerType>, IRule<TReturnedField>> action)
    {
        if (rule is ValidRule<IEnumerable<TInnerType>> validRule)
        {
            var fieldCollection = validRule.FieldValue;
            var fieldFullPath = validRule.Context.FieldName;
            var index = 0;

            foreach (var innerField in fieldCollection)
            {
                var newRule = ValidRule<TInnerType>.CreateFromField(
                    innerField,
                    fieldFullPath,
                    validRule.Context,
                    index,
                    validRule.Context.ParentPath);
                var ruleReturned = action(newRule);

                if (ruleReturned is InvalidRule<TReturnedField>)
                {
                    return new InvalidRule<IEnumerable<TInnerType>>();
                }
                
                index++;
            }
        }

        return rule;
    }
}