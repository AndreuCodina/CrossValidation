using System.Numerics;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public static class RuleExtensions
{
    public static Rule<TField> NotNull<TField>(
        this Rule<TField?> rule)
        where TField : class
    {
        return rule.SetValidator(() => new NotNullValidator<TField?>(rule.FieldValue))!;
    }
    
    public static Rule<TField> NotNull<TField>(
        this Rule<TField?> rule)
        where TField : struct
    {
        return rule.SetValidator(() => new NotNullValidator<TField?>(rule.FieldValue))
            .Transform(x => x ?? default);
    }
    
    public static Rule<TField?> Null<TField>(
        this Rule<TField?> rule)
        where TField : class
    {
        return rule.SetValidator(() => new NullValidator<TField?>(rule.FieldValue));
    }
    
    public static Rule<TField?> Null<TField>(
        this Rule<TField?> rule)
        where TField : struct
    {
        return rule.SetValidator(() => new NullValidator<TField?>(rule.FieldValue));
    }
    
    public static Rule<TField> GreaterThan<TField>(
        this Rule<TField> rule,
        TField valueToCompare)
        where TField : IComparisonOperators<TField, TField, bool>
    {
        return rule.SetValidator(() => new GreaterThanValidator<TField>(rule.FieldValue!, valueToCompare));
    }

    public static Rule<TField> IsInEnum<TField>(
        this Rule<TField> rule)
        where TField : Enum
    {
        return rule.SetValidator(() => new EnumValidator<TField>(rule.FieldValue!, typeof(TField)));
    }
    
    public static Rule<int> IsInEnum(
        this Rule<int> rule,
        Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new InvalidOperationException($"Cannot use {nameof(IsInEnum)} if the type provided is not an enumeration");
        }
        
        return rule.SetValidator(() => new EnumValidator<int>(rule.FieldValue!, enumType));
    }
    
    public static Rule<string> IsInEnum(
        this Rule<string> rule,
        Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new InvalidOperationException($"Cannot use {nameof(IsInEnum)} if the type provided is not an enumeration");
        }
        
        return rule.SetValidator(() => new EnumValidator<string>(rule.FieldValue!, enumType));
    }
    
    public static Rule<string> Length(
        this Rule<string> rule,
        int minimum,
        int maximum)
    {
        return rule.SetValidator(() => new LengthRangeValidator(rule.FieldValue!, minimum, maximum));
    }
    
    public static CollectionRule<IEnumerable<TInnerType>> ForEach<TInnerType>(
        this CollectionRule<IEnumerable<TInnerType>> rule,
        Action<Rule<TInnerType>> action)
    {
        var fieldCollection = rule.FieldValue!;
        var fieldFullPath = rule.Context.FieldName;
        var index = 0;
        
        foreach (var innerField in fieldCollection)
        {
            var newRule = new Rule<TInnerType>(
                innerField,
                fieldFullPath: fieldFullPath,
                rule.Context,
                index,
                parentPath: rule.Context.ParentPath);
            action(newRule);
            index++;
        }
    
        return rule;
    }
}