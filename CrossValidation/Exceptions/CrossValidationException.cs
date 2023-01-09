using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

public class CrossValidationException : Exception
{
    public List<IValidationError> Errors { get; }

    public CrossValidationException(List<IValidationError> errors)
    {
        Errors = errors;
    }
}