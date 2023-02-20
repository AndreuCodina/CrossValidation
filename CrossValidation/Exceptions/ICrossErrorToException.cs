using CrossValidation.Errors;

namespace CrossValidation.Exceptions;

/// <summary>
/// Implemented by exceptions that use CrossValidation DSL
/// </summary>
public interface ICrossErrorToException
{
    static abstract Exception FromCrossError(ICrossError error);
}