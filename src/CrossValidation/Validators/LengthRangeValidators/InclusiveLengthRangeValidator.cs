using CrossValidation.Exceptions;

namespace CrossValidation.Validators.LengthRangeValidators;

public class InclusiveLengthRangeValidator(string fieldValue, int minimum, int maximum)
    : LengthRangeValidator(minimum, maximum)
{
    private readonly string _fieldValue = fieldValue;
    private readonly int _minimum = minimum;
    private readonly int _maximum = maximum;

    public override bool IsValid()
    {
        return _fieldValue.Length >= _minimum
               && _fieldValue.Length <= _maximum;
    }

    public override BusinessException CreateException()
    {
        return new CommonException.InclusiveLengthRangeException(_minimum, _maximum, _fieldValue.Length);
    }
}