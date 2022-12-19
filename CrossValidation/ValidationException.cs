using CrossValidation.Results;

namespace CrossValidation;

public class ValidationException : Exception
{
    public List<ValidationError> Errors { get; }

    public ValidationException(List<ValidationError> errors)
    {
        Errors = errors;
    }
}