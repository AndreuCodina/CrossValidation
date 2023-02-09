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

    public static ICrossError ShouldThrowValidationError(this Action actual, string? customMessage = null)
    {
        return actual.ShouldThrow<CrossException>().Error;
    }
    
    public static ICrossError ShouldThrowValidationError(this Func<object?> actual, string? customMessage = null)
    {
        return actual.ShouldThrow<CrossException>().Error;
    }
    
    public static TValidationError ShouldThrowValidationError<TValidationError>(this Action actual, string? customMessage = null)
        where TValidationError : ICrossError
    {
        var error = actual.ShouldThrow<CrossException>().Error;
        return error.ShouldBeOfType<TValidationError>(customMessage);
    }
    
    public static TValidationError ShouldThrowValidationError<TValidationError>(this Func<object?> actual, string? customMessage = null)
        where TValidationError : ICrossError
    {
        var error = actual.ShouldThrow<CrossException>().Error;
        return error.ShouldBeOfType<TValidationError>(customMessage);
    }
    
    public static List<ICrossError> ShouldThrowValidationErrors(this Action actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<ValidationListException>().Errors;
        return errors;
    }
    
    public static List<ICrossError> ShouldThrowValidationErrors(this Func<object?> actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<ValidationListException>().Errors;
        return errors;
    }
}