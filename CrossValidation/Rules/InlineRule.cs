using CrossValidation.Results;
using CrossValidation.ValidationContexts;

namespace CrossValidation.Rules;

public class InlineRule<TField>
    : Rule<
        InlineRule<TField>,
        TField,
        InlineValidationContext
    >
{
    public InlineRule(TField value)
    {
        // TODO
        FieldValue = value;
        Context = new InlineValidationContext();
    }

    protected override InlineRule<TField> GetSelf()
    {
        return this;
    }

    protected override void HandleError(ValidationError error)
    {
        Context.AddError(error);
        throw new ValidationException(Context.Errors!);
    }
}