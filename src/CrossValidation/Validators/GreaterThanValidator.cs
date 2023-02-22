using System.Numerics;
using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record GreaterThanValidator<TField>(
    TField FieldValue,
    TField ComparisonValue) : Validator
    where TField : IComparisonOperators<TField, TField, bool>
{
    public override bool IsValid()
    {
        return FieldValue > ComparisonValue;
    }

    public override CrossError CreateError()
    {
        return new CommonCrossError.GreaterThan<TField>(ComparisonValue);
    }
}