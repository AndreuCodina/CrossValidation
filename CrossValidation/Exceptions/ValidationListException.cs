using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

public class ValidationListException : NoStackTraceException
{
    public List<ICrossError> Errors { get; }

    public ValidationListException(List<ICrossError> errors)
    {
        Errors = errors;
    }
}