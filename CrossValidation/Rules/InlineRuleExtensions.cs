using CrossValidation.Validators;

namespace CrossValidation.Rules;

public static class InlineRuleExtensions
{
    public static InlineRule<TField> NotNull<TField>(
        this InlineRule<TField?> rule)
        where TField : class
    {
        return rule.SetValidator(() => new NotNullValidator<TField?>(rule.FieldValue))!;
    }
    
    public static InlineRule<TField> NotNull<TField>(
        this InlineRule<TField?> rule)
        where TField : struct
    {
        return rule.SetValidator(() => new NotNullValidator<TField?>(rule.FieldValue))
            .Transform(x => x!.Value);
    }
    
    public static InlineRule<TField?> Null<TField>(
        this InlineRule<TField?> rule)
        where TField : class
    {
        return rule.SetValidator(() => new NullValidator<TField?>(rule.FieldValue));
    }
    
    public static InlineRule<TField?> Null<TField>(
        this InlineRule<TField?> rule)
        where TField : struct
    {
        return rule.SetValidator(() => new NullValidator<TField?>(rule.FieldValue));
    }
}