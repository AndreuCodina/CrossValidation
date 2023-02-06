namespace CrossValidation.Exceptions;

public class NoStackTraceException : Exception
{
    public override string? StackTrace { get; } = null;
}