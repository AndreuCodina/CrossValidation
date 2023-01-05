using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

public class CrossValidationException : Exception
{
    public List<ICrossValidationError> Errors { get; }

    public CrossValidationException(List<ICrossValidationError> errors)
    {
        Errors = errors;
    }
}