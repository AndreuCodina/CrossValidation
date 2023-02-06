using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

public class ValidationException : NoStackTraceException
{
    public IValidationError Error { get; }

    public ValidationException(IValidationError error)
    {
        Error = error;
    }
}