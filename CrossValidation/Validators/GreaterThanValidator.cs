using System.Numerics;
using CrossValidation.Results;

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

    public override ValidationError CreateError()
    {
        return new CommonValidationError.GreaterThan<TField>(ComparisonValue);
    }
}