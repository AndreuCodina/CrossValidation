using CrossValidation.Results;

namespace CrossValidation.FieldValidators;

public record GreaterThanValidator<TField>(
    TField FieldValue,
    TField ComparisonValue) : FieldValidator
    where TField : IComparable<TField>
{
    protected override bool IsValid()
    {
        return FieldValue.CompareTo(ComparisonValue) > 0;
    }

    protected override ValidationError CreateError()
    {
        return new CommonValidationError.GreaterThan<TField>(ComparisonValue);
    }
}