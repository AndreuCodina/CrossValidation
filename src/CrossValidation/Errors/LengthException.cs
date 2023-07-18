using CrossValidation.Exceptions;

namespace CrossValidation.Errors;

public abstract class LengthException(string code) : BusinessException(code: code)
{
    public abstract int TotalLength { get; }
}