using System.Diagnostics.Contracts;
using CrossValidation.Rules;
using CrossValidation.Utils;

namespace CrossValidation;

public static class Ensure
{
    [Pure]
    public static IRule<TField> That<TField>(TField fieldValue)
    {
        return IValidRule<TField>.CreateFromField(dsl: Dsl.Ensure, fieldValue);
    }
    
    public static void ModelNullability<TModel>(TModel model)
    {
        ModelNullabilityValidator.Validate(model);
    }
}