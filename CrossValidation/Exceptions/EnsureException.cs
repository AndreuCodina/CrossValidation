using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

public class EnsureException : NoStackTraceException
{
    public EnsureError Error { get; }

    internal EnsureException(EnsureError error)
    {
        Error = error;
    }
}