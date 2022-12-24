using System.Numerics;
using CrossValidation.ValidationContexts;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public static class RuleExtensions
{
    public static TSelf Null<TSelf, TField, TValidationContext>(
        this Rule<TSelf, TField,TValidationContext> rule)
        where TValidationContext : ValidationContext
    {
        return rule.SetValidator(new NullValidator<TField>(rule.FieldValue));
    }

    public static TSelf GreaterThan<TSelf, TField, TValidationContext>(
        this Rule<TSelf, TField, TValidationContext> rule,
        TField valueToCompare)
        where TValidationContext : ValidationContext
        where TField : IComparisonOperators<TField, TField, bool>
    {
        return rule.SetValidator(new GreaterThanValidator<TField>(rule.FieldValue!, valueToCompare));
    }

    public static TSelf IsInEnum<TSelf, TField, TValidationContext>(
        this Rule<TSelf, TField, TValidationContext> rule)
        where TValidationContext : ValidationContext
        where TField : Enum
    {
        return rule.SetValidator(new EnumValidator<TField>(rule.FieldValue!, typeof(TField)));
    }
    
    public static TSelf IsInEnum<TSelf, TValidationContext>(
        this Rule<TSelf, int, TValidationContext> rule,
        Type enumType)
        where TValidationContext : ValidationContext
    {
        if (!enumType.IsEnum)
        {
            throw new InvalidOperationException($"Cannot use {nameof(IsInEnum)} if the type provided is not an enumeration");
        }
        
        return rule.SetValidator(new EnumValidator<int>(rule.FieldValue!, enumType));
    }
    
    public static TSelf IsInEnum<TSelf, TValidationContext>(
        this Rule<TSelf, string, TValidationContext> rule,
        Type enumType)
        where TValidationContext : ValidationContext
    {
        if (!enumType.IsEnum)
        {
            throw new InvalidOperationException($"Cannot use {nameof(IsInEnum)} if the type provided is not an enumeration");
        }
        
        return rule.SetValidator(new EnumValidator<string>(rule.FieldValue!, enumType));
    }
    
    public static TSelf Length<TSelf, TValidationContext>(
        this Rule<TSelf, string, TValidationContext> rule,
        int minimum,
        int maximum)
        where TValidationContext : ValidationContext
    {
        return rule.SetValidator(new LengthRangeValidator(rule.FieldValue!, minimum, maximum));
    }
}