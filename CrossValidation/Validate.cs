using CrossValidation.Rules;

namespace CrossValidation;

public static class Validate
{
    public static InlineRule<TField> That<TField>(TField fieldValue)
    {
        return new InlineRule<TField>(fieldValue);
    }
}