using CrossValidation.Errors;

namespace CrossValidation.Results;

public interface ILengthError : ICrossValidationError
{
    public int TotalLength { get; }
}