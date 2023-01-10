using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

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
}