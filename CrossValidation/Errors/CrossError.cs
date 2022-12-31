namespace CrossValidation.Results;

public record CrossError
{
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