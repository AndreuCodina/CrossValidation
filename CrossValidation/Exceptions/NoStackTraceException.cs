namespace CrossValidation.Exceptions;

public class NoStackTraceException : Exception
{
    public NoStackTraceException()
    {
    }
    
    public NoStackTraceException(string? message) : base(message)
    {
    }

    public override string? StackTrace { get; } = null;
}