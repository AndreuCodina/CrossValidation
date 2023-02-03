using System.Diagnostics.Contracts;
using CrossValidation.Rules;

namespace CrossValidation;

public static class Ensure
{
    [Pure]
    public static IRule<TField> That<TField>(TField fieldValue)
    {
        return IValidRule<TField>.CreateFromField(dsl: Dsl.Ensure, fieldValue);
    }
}