using System.Numerics;
using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class GreaterThanValidator<TField>(
    TField fieldValue,
    TField comparisonValue) : Validator
    where TField : IComparisonOperators<TField, TField, bool>
{
    public override bool IsValid()
    {
        return fieldValue > comparisonValue;
    }

    public override BusinessException CreateException()
    {
        return new CommonCrossException.GreaterThan<TField>(comparisonValue);
    }
}