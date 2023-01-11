using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

/// <summary>
/// Common exception for non-exceptional situations
/// </summary>
public class CrossException : Exception
{
    public Error Error { get; }
    public string? MessageDescription { get; }

    internal CrossException(Error error, string? messageDescription = null) :
        base(message: error.BuildMessage(messageDescription))
    {
        Error = error;
        MessageDescription = messageDescription;
    }
    
#if !DEBUG
    public override string? StackTrace { get; } = null;
#endif
}