using CrossValidation.Exceptions;

namespace CrossValidation.Results;

public record CrossError
{
    public CrossException ToException()
    {
        return new CrossException(this);
    }
    
    public string BuildMessage(string? messageDescription = null)
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