namespace CrossValidation.Exceptions;

public class MessageCrossError(string message) :
    BusinessException(message: message)
{
    public MessageCrossError(string message, params object[] messageParameters)
        : this(string.Format(message, messageParameters))
    {
    }
}