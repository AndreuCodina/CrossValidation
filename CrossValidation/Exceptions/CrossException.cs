using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

public class CrossException : NoStackTraceException
{
    public ICrossError Error { get; }

    public CrossException(ICrossError error)
    {
        Error = error;
    }
}