using System.Numerics;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public static class RuleExtensions
{
    public static void NotNull<TField>(
        this IRule<TField?> rule)
        where TField : class
    {
        rule.SetValidator(() => new NotNullValidator<TField?>(rule.GetFieldValue()));
    }
    
    public static void NotNull<TField>(
        this IRule<TField?> rule,
        Action<IRule<TField>> notNullRule)
        where TField : class
    {
        var validator = new NotNullValidator<TField?>(rule.GetFieldValue());
        rule.SetValidator(() => validator);

        if (validator.IsValid())
        {
            notNullRule(rule.Transform(x => x!));
        }
    }
    
    public static void NotNull<TField>(
        this IRule<TField?> rule)
        where TField : struct
    {
        rule.SetValidator(() => new NotNullValidator<TField?>(rule.GetFieldValue()));
    }
    
    public static void NotNull<TField>(
        this IRule<TField?> rule,
        Action<IRule<TField>> notNullRule)
        where TField : struct
    {
        var validator = new NotNullValidator<TField?>(rule.GetFieldValue());
        rule.SetValidator(() => validator);

        if (validator.IsValid())
        {
            notNullRule(rule.Transform(x => x!.Value));
        }
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
    
    public static IRule<int> IsInEnum(
        this IRule<int> rule,
        Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new InvalidOperationException($"Cannot use {nameof(IsInEnum)} if the type provided is not an enumeration");
        }
        
        return rule.SetValidator(() => new EnumValidator<int>(rule.GetFieldValue(), enumType));
    }
    
    public static IRule<string> IsInEnum(
        this IRule<string> rule,
        Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new InvalidOperationException($"Cannot use {nameof(IsInEnum)} if the type provided is not an enumeration");
        }
        
        return rule.SetValidator(() => new EnumValidator<string>(rule.GetFieldValue(), enumType));
    }
    
    public static IRule<string> Length(
        this IRule<string> rule,
        int minimum,
        int maximum)
    {
        return rule.SetValidator(() => new LengthRangeValidator(rule.GetFieldValue(), minimum, maximum));
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
                innerField,
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