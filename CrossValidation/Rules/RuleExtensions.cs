using System.Numerics;
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

        if (rule is IValidRule<TField?> validRule)
        {
            ruleToReturn = validRule.SetValidator(new NotNullValidator<TField?>(validRule.GetFieldValue()));
        }

        return ruleToReturn.Transform(x => x!);
    }

    public static IRule<TField> NotNull<TField>(
        this IRule<TField?> rule)
        where TField : struct
    {
        var ruleToReturn = rule;

        if (rule is IValidRule<TField?> validRule)
        {
            ruleToReturn = validRule.SetValidator(new NotNullValidator<TField?>(validRule.GetFieldValue()));
        }

        return ruleToReturn.Transform(x => x!.Value);
    }

    public static IRule<TField?> WhenNotNull<TField, TReturnedField>(
        this IRule<TField?> rule,
        Func<IRule<TField>, IRule<TReturnedField>> notNullRule)
        where TField : struct
    {
        if (rule is not IValidRule<TField?> validRule)
        {
            return rule;
        }

        if (validRule.GetFieldValue() is not null)
        {
            var ruleReturned = notNullRule(rule.Transform(x => x!.Value));

            if (ruleReturned is IInvalidRule<TReturnedField>)
            {
                return IInvalidRule<TField?>.Create();
            }
        }

        return rule;
    }

    public static IRule<TField?> WhenNotNull<TField, TReturnedField>(
        this IRule<TField?> rule,
        Func<IRule<TField>, IRule<TReturnedField>> notNullRule)
        where TField : class
    {
        if (rule is not IValidRule<TField?> validRule)
        {
            return rule;
        }

        if (validRule.GetFieldValue() is not null)
        {
            var ruleReturned = notNullRule(rule.Transform(x => x!));

            if (ruleReturned is IInvalidRule<TReturnedField>)
            {
                return IInvalidRule<TField?>.Create();
            }
        }

        return rule;
    }

    public static IRule<TField?> Null<TField>(
        this IRule<TField?> rule)
        where TField : class
    {
        if (rule is IValidRule<TField?> validRule)
        {
            return rule.SetValidator(new NullValidator<TField?>(validRule.GetFieldValue()));
        }

        return rule;
    }

    public static IRule<TField?> Null<TField>(
        this IRule<TField?> rule)
        where TField : struct
    {
        if (rule is IValidRule<TField?> validRule)
        {
            return rule.SetValidator(new NullValidator<TField?>(validRule.GetFieldValue()));
        }

        return rule;
    }

    public static IRule<TField> GreaterThan<TField>(
        this IRule<TField> rule,
        TField valueToCompare)
        where TField : IComparisonOperators<TField, TField, bool>
    {
        if (rule is IValidRule<TField> validRule)
        {
            return rule.SetValidator(new GreaterThanValidator<TField>(validRule.GetFieldValue(), valueToCompare));
        }

        return rule;
    }

    public static IRule<TField> Enum<TField>(
        this IRule<TField> rule)
        where TField : Enum
    {
        if (rule is IValidRule<TField> validRule)
        {
            return rule.SetValidator(new EnumValidator<TField>(validRule.GetFieldValue(), typeof(TField)));
        }

        return rule;
    }

    public static IRule<TEnum> Enum<TEnum>(
        this IRule<int> rule)
        where TEnum : Enum
    {
        var ruleToReturn = rule;
        
        if (rule is IValidRule<int> validRule)
        {
            ruleToReturn = validRule.SetValidator(new EnumValidator<int>(validRule.GetFieldValue(), typeof(TEnum)));
        }
        
        return ruleToReturn.Transform(x => (TEnum)(object)x);
    }

    public static IRule<TEnum> Enum<TEnum>(
        this IRule<string> rule)
        where TEnum : Enum
    {
        var ruleToReturn = rule;
        
        if (rule is IValidRule<string> validRule)
        {
            ruleToReturn = validRule.SetValidator(new EnumValidator<string>(validRule.GetFieldValue(), typeof(TEnum)));
        }

        return ruleToReturn.Transform(x => (TEnum)System.Enum.Parse(typeof(TEnum), x));
    }

    public static IRule<string> LengthRange(
        this IRule<string> rule,
        int minimum,
        int maximum)
    {
        if (rule is IValidRule<string> validRule)
        {
            return rule.SetValidator(new LengthRangeValidator(validRule.GetFieldValue(), minimum, maximum));
        }

        return rule;
    }

    public static IRule<string> MinimumLength(
        this IRule<string> rule,
        int minimum)
    {
        if (rule is IValidRule<string> validRule)
        {
            return rule.SetValidator(new MinimumLengthValidator(validRule.GetFieldValue(), minimum));
        }

        return rule;
    }

    public static IRule<IEnumerable<TInnerType>> ForEach<TInnerType, TReturnedField>(
        this IRule<IEnumerable<TInnerType>> rule,
        Func<IRule<TInnerType>, IRule<TReturnedField>> action)
    {
        if (rule is IValidRule<IEnumerable<TInnerType>> validRule)
        {
            var fieldCollection = validRule.GetFieldValue();
            var fieldFullPath = validRule.Context.FieldName;
            var index = 0;

            foreach (var innerField in fieldCollection)
            {
                var newRule = IValidRule<TInnerType>.CreateFromField(
                    innerField,
                    fieldFullPath,
                    validRule.Context,
                    index,
                    validRule.Context.ParentPath);
                var ruleReturned = action(newRule);

                if (ruleReturned is IInvalidRule<TReturnedField>)
                {
                    return IInvalidRule<IEnumerable<TInnerType>>.Create();
                }

                index++;
            }
        }

        return rule;
    }
}