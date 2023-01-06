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
        rule.SetValidator(() => new NotNullValidator<TField?>(rule.GetFieldValue()));
        return rule.Transform(x => x!);
    }

    public static IRule<TField> NotNull<TField>(
        this IRule<TField?> rule)
        where TField : struct
    {
        rule.SetValidator(() => new NotNullValidator<TField?>(rule.GetFieldValue()));
        return rule.Transform(x => x!.Value);
    }

    public static IRule<TField?> WhenNotNull<TField>(
        this IRule<TField?> rule,
        Action<IRule<TField>> notNullRule)
        where TField : struct
    {
        var validator = new NotNullValidator<TField?>(rule.GetFieldValue());

        if (validator.IsValid())
        {
            notNullRule(rule.Transform(x => x!.Value));
        }

        return rule;
    }

    public static IRule<TField?> WhenNotNull<TField>(
        this IRule<TField?> rule,
        Action<IRule<TField>> notNullRule)
        where TField : class
    {
        var validator = new NotNullValidator<TField?>(rule.GetFieldValue());

        if (validator.IsValid())
        {
            notNullRule(rule.Transform(x => x!));
        }

        return rule;
    }

    public static IRule<TField?> Null<TField>(
        this IRule<TField?> rule)
        where TField : class
    {
        return rule.SetValidator(() => new NullValidator<TField?>(rule.GetFieldValue()));
    }

    public static IRule<TField?> Null<TField>(
        this IRule<TField?> rule)
        where TField : struct
    {
        return rule.SetValidator(() => new NullValidator<TField?>(rule.GetFieldValue()));
    }

    public static IRule<TField> GreaterThan<TField>(
        this IRule<TField> rule,
        TField valueToCompare)
        where TField : IComparisonOperators<TField, TField, bool>
    {
        return rule.SetValidator(() => new GreaterThanValidator<TField>(rule.GetFieldValue(), valueToCompare));
    }

    public static IRule<TField> IsInEnum<TField>(
        this IRule<TField> rule)
        where TField : Enum
    {
        return rule.SetValidator(() => new EnumValidator<TField>(rule.GetFieldValue(), typeof(TField)));
    }

    public static IRule<int> IsInEnum<TEnum>(
        this IRule<int> rule)
        where TEnum : Enum
    {
        return rule.SetValidator(() => new EnumValidator<int>(rule.GetFieldValue(), typeof(TEnum)));
    }

    public static IRule<string> IsInEnum<TEnum>(
        this IRule<string> rule)
        where TEnum : Enum
    {
        return rule.SetValidator(() => new EnumValidator<string>(rule.GetFieldValue(), typeof(TEnum)));
    }

    public static IRule<string> LengthRange(
        this IRule<string> rule,
        int minimum,
        int maximum)
    {
        return rule.SetValidator(() => new LengthRangeValidator(rule.GetFieldValue(), minimum, maximum));
    }
    
    public static IRule<string> MinimumLength(
        this IRule<string> rule,
        int minimum)
    {
        return rule.SetValidator(() => new MinimumLengthValidator(rule.GetFieldValue(), minimum));
    }

    public static IRule<IEnumerable<TInnerType>> ForEach<TInnerType>(
        this IRule<IEnumerable<TInnerType>> rule,
        Action<IRule<TInnerType>> action)
    {
        var fieldCollection = rule.GetFieldValue();
        var fieldFullPath = rule.Context.FieldName;
        var index = 0;

        foreach (var innerField in fieldCollection)
        {
            var newRule = Rule<TInnerType>.CreateFromField(
                () => innerField,
                rule.State,
                fieldFullPath,
                rule.Context,
                index,
                rule.Context.ParentPath);
            action(newRule);
            index++;
        }

        return rule;
    }
}