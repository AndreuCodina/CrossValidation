using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

public class CrossException : NoStackTraceException, ICrossErrorToException
{
    public ICrossError Error { get; }

    public CrossException(ICrossError error)
    {
        Error = error;
    }

    public static Exception FromCrossError(ICrossError error)
    {
        return new CrossException(error);
    }
}