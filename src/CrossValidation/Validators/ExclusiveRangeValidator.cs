using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class ExclusiveRangeValidator<TField>(TField fieldValue, TField from, TField to) : Validator
    where TField : IComparable<TField>
{
    private readonly TField _fieldValue = fieldValue;
    private readonly TField _from = from;
    private readonly TField _to = to;

    public override bool IsValid()
    {
        return _fieldValue.CompareTo(_from) > 0
               && _fieldValue.CompareTo(_to) < 0;
    }

    public override BusinessException CreateException()
    {
        return new CommonException.ExclusiveRangeException<TField>(_from, _to);
    }
}