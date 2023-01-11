using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

/// <summary>
/// Contains data to show to the user
/// </summary>
public class CrossValidationException : Exception
{
    public List<IValidationError> Errors { get; }

    public CrossValidationException(List<IValidationError> errors)
    {
        Errors = errors;
    }

#if !DEBUG
    public override string? StackTrace { get; } = null;
#endif
}