using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

/// <summary>
/// Contains data to show to the user
/// </summary>
public class CrossValidationException : NoStackTraceException
{
    public List<IValidationError> Errors { get; }

    public CrossValidationException(List<IValidationError> errors)
    {
        Errors = errors;
    }
}