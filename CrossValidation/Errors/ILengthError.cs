using CrossValidation.Errors;

namespace CrossValidation.Results;

public interface ILengthError : IValidationError
{
    public int TotalLength { get; }
}