using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

public class CrossArgumentException : Exception, ICrossErrorToException
{
    public CrossArgumentException(string? message) : base(message)
    {
    }

    public static Exception FromCrossError(ICrossError error)
    {
        string? message = null;
            
        if (error.FieldName is not null)
        {
            message = $"{error.FieldName}: ";
        }

        if (error.Message is not null)
        {
            if (message is not null)
            {
                message += error.Message;
            }
            else
            {
                message = error.Message;
            }
        }
            
        return new CrossArgumentException(message);
    }
}