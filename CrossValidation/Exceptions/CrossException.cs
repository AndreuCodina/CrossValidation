using CrossValidation.Results;

namespace CrossValidation.Exceptions;

public class CrossException : Exception
{
    public CrossError Error { get; }
    public string? MessageDescription { get; }

    internal CrossException(CrossError error, string? messageDescription = null) :
        base(message: error.BuildMessage(messageDescription))
    {
        Error = error;
        MessageDescription = messageDescription;
    }
}