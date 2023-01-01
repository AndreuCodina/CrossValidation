using CrossValidation.Results;

namespace CrossValidation.Exceptions;

public class CrossException : Exception
{
    public CrossError Error { get; }
    
    public CrossException(CrossError error) : base(message: error.BuildMessage())
    {
        Error = error;
    }
    
    public CrossException(CrossError error, string messageDescription) :
        base(message: error.BuildMessage(messageDescription))
    {
        Error = error;
    }
}