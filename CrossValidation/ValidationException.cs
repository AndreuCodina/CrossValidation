using CrossValidation.Results;

namespace CrossValidation;

public class ValidationException : Exception
{
    public List<CrossValidationError> Errors { get; }

    public ValidationException(List<CrossValidationError> errors)
    {
        Errors = errors;
    }
}