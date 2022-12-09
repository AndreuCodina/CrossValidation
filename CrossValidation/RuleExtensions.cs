using System.Numerics;
using CrossValidation.FieldValidators;
using CrossValidation.Rules;
using CrossValidation.ValidationContexts;

namespace CrossValidation;

public static class RuleExtensions
{
    public static TSelf NotNull<TSelf, TField, TValidationContext>(
        this Rule<TSelf, TField, TValidationContext> rule)
        where TValidationContext : ValidationContext
    {
        return rule.SetValidator(new NotNullValidator<TField>(rule.FieldValue));
    }
    
    public static TSelf Null<TSelf, TField, TValidationContext>(
        this Rule<TSelf, TField, TValidationContext> rule)
        where TValidationContext : ValidationContext
    {
        return rule.SetValidator(new NullValidator<TField>(rule.FieldValue));
    }

    public static TSelf GreaterThan<TSelf, TField, TValidationContext>(
        this Rule<TSelf, TField, TValidationContext> rule, TField valueToCompare)
        where TValidationContext : ValidationContext
        where TField : IComparisonOperators<TField, TField, bool>
    {
        return rule.SetValidator(new GreaterThanValidator<TField>(rule.FieldValue!, valueToCompare));
    }
}