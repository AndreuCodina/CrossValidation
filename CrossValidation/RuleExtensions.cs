using CrossValidation.FieldValidators;
using CrossValidation.Rules;
using CrossValidation.ValidationContexts;

namespace CrossValidation;

public static class RuleExtensions
{
    // TODO:
    // User-defined data:
    // - Create extension method for IRule that accepts your class/interface that define the error for your system
    // .SetValidator(ICustomError error), and that method assign fields to the Dictionary<object, object>
    // and call the CrossValidation validator
    // and customize the response from ValidationException to ProblemDetails

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
        where TField : IComparable<TField>
    {
        return rule.SetValidator(new GreaterThanValidator<TField>(rule.FieldValue, valueToCompare));
    }
}