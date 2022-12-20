using CrossValidation.Results;
using CrossValidation.ValidationContexts;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public class InlineRule<TField>
    : Rule<
        InlineRule<TField>,
        TField,
        InlineValidationContext>
{
    public InlineRule(TField? value)
    {
        // TODO
        FieldValue = value;
        Context = new InlineValidationContext();
    }

    protected override InlineRule<TField> GetSelf()
    {
        return this;
    }

    protected override void HandleError(CrossValidationError error)
    {
        Context.AddError(error);
        throw new ValidationException(Context.Errors!);
    }

    public InlineRule<TField> When(bool condition)
    {
        Context.ExecuteNextValidator = condition;
        return this;
    }
    
    public InlineRule<TField> When(Func<TField?, bool> condition)
    {
        Context.ExecuteNextValidator = condition(FieldValue);
        return this;
    }
    
    public InlineRule<TField> Must(Func<TField?, bool> condition)
    {
        SetValidator(new PredicateValidator(condition(FieldValue)));
        return this;
    }
}