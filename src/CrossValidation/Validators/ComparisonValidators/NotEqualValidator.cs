using CrossValidation.Exceptions;

namespace CrossValidation.Validators.ComparisonValidators;

public class NotEqualValidator<TField>(
    TField fieldValue,
    TField comparisonValue) : Validator
    where TField : IEquatable<TField>
{
    public override bool IsValid()
    {
        return !fieldValue.Equals(comparisonValue);
    }

    public override BusinessException CreateException()
    {
        return new CommonException.NotEqualException<TField>(comparisonValue);
    }
}