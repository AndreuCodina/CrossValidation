using CrossValidation.Exceptions;

namespace CrossValidation.Results;

public record CrossError
{
    public CrossException ToException(string? messageDescription = null)
    {
        return new CrossException(this, messageDescription);
    }

    internal string BuildMessage(string? messageDescription = null)
    {
        var errorStructure = ToString();
        var message = errorStructure;
        
        if (messageDescription is not null)
        {
            message += $". {messageDescription}";
        }
        
        return message;
    }
}