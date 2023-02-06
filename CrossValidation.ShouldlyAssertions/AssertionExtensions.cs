using CrossValidation.Errors;
using CrossValidation.Exceptions;
using Shouldly;

namespace CrossValidation.ShouldlyAssertions;

[ShouldlyMethods]
public static class AssertionExtensions
{
    public static EnsureError ShouldThrowEnsureError(this Action actual, string? customMessage = null)
    {
        return actual.ShouldThrow<EnsureException>(customMessage)
            .Error;
    }
    
    public static TError ShouldThrowEnsureError<TError>(this Action actual, string? customMessage = null)
        where TError : EnsureError
    {
        return actual.ShouldThrow<EnsureException>()
            .Error
            .ShouldBeOfType<TError>(customMessage);
    }
    
    public static EnsureError ShouldThrowEnsureError(this Func<object?> actual, string? customMessage = null)
    {
        return actual.ShouldThrow<EnsureException>(customMessage)
            .Error;
    }

    public static TError ShouldThrowEnsureError<TError>(this Func<object?> actual, string? customMessage = null)
        where TError : EnsureError
    {
        return actual.ShouldThrow<EnsureException>()
            .Error
            .ShouldBeOfType<TError>(customMessage);
    }

    public static IValidationError ShouldThrowValidationError(this Action actual, string? customMessage = null)
    {
        return actual.ShouldThrow<ValidationException>().Error;
    }
    
    public static IValidationError ShouldThrowValidationError(this Func<object?> actual, string? customMessage = null)
    {
        return actual.ShouldThrow<ValidationException>().Error;
    }
    
    public static TValidationError ShouldThrowValidationError<TValidationError>(this Action actual, string? customMessage = null)
        where TValidationError : IValidationError
    {
        var error = actual.ShouldThrow<ValidationException>().Error;
        return error.ShouldBeOfType<TValidationError>(customMessage);
    }
    
    public static TValidationError ShouldThrowValidationError<TValidationError>(this Func<object?> actual, string? customMessage = null)
        where TValidationError : IValidationError
    {
        var error = actual.ShouldThrow<ValidationException>().Error;
        return error.ShouldBeOfType<TValidationError>(customMessage);
    }
    
    public static List<IValidationError> ShouldThrowValidationErrors(this Action actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<ValidationListException>().Errors;
        return errors;
    }
    
    public static List<IValidationError> ShouldThrowValidationErrors(this Func<object?> actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<ValidationListException>().Errors;
        return errors;
    }
}