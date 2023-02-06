using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

public class ValidationListException : NoStackTraceException
{
    public List<IValidationError> Errors { get; }

    public ValidationListException(List<IValidationError> errors)
    {
        Errors = errors;
    }
}