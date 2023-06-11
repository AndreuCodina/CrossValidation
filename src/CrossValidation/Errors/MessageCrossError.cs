namespace CrossValidation.Errors;

public record MessageCrossError(string Message) :
    CompleteCrossError(Message: Message)
{
    public MessageCrossError(string Message, params object[] Parameters) : this(string.Format(Message, Parameters))
    {
    }
}