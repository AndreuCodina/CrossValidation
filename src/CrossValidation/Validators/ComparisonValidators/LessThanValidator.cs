using CrossValidation.Exceptions;

namespace CrossValidation.Validators.ComparisonValidators;

public class LessThanValidator<TField>(
    TField fieldValue,
    TField comparisonValue) : Validator
    where TField : IComparable<TField>
{
    public override bool IsValid()
    {
        return fieldValue.CompareTo(comparisonValue) < 0;
    }

    public override BusinessException CreateException()
    {
        return new CommonException.LessThanException<TField>(comparisonValue);
    }
}