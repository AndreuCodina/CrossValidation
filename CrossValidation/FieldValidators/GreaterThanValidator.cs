using System.Numerics;
using CrossValidation.Results;

namespace CrossValidation.FieldValidators;

public record GreaterThanValidator<TField>(
    TField FieldValue,
    TField ComparisonValue) : FieldValidator
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