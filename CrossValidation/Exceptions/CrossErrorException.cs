using CrossValidation.Results;

namespace CrossValidation.Exceptions;

public class CrossErrorException : Exception
{
    public CrossError Error { get; }
    
    public CrossErrorException(CrossError error) : base(message: error.BuildMessage())
    {
        Error = error;
    }
    
    public CrossErrorException(CrossError error, string messageDescription) :
        base(message: error.BuildMessage(messageDescription))
    {
        Error = error;
    }
}