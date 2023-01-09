using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Results;
using Shouldly;

namespace CrossValidation.ShouldlyAssertions;

[ShouldlyMethods]
public static class AssertionExtensions
{
    public static Error ShouldThrowError(this Action actual, string? customMessage = null)
    {
        return actual.ShouldThrow<CrossException>(customMessage)
            .Error;
    }
    
    public static TError ShouldThrowError<TError>(this Action actual, string? customMessage = null)
        where TError : Error
    {
        return actual.ShouldThrow<CrossException>()
            .Error
            .ShouldBeOfType<TError>(customMessage);
    }
    
    public static Error ShouldThrowError(this Func<object?> actual, string? customMessage = null)
    {
        return actual.ShouldThrow<CrossException>(customMessage)
            .Error;
    }

    public static TError ShouldThrowError<TError>(this Func<object?> actual, string? customMessage = null)
        where TError : Error
    {
        return actual.ShouldThrow<CrossException>()
            .Error
            .ShouldBeOfType<TError>(customMessage);
    }

    public static IValidationError ShouldThrowValidationError(this Action actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        return errors[0];
    }
    
    public static IValidationError ShouldThrowValidationError(this Func<object?> actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        return errors[0];
    }
    
    public static TValidationError ShouldThrowValidationError<TValidationError>(this Action actual, string? customMessage = null)
        where TValidationError : IValidationError
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        var error = errors[0];
        return error.ShouldBeOfType<TValidationError>(customMessage);
    }
    
    public static TValidationError ShouldThrowValidationError<TValidationError>(this Func<object?> actual, string? customMessage = null)
        where TValidationError : IValidationError
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        var error = errors[0];
        return error.ShouldBeOfType<TValidationError>(customMessage);
    }
    
    public static List<IValidationError> ShouldThrowValidationErrors(this Action actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBeGreaterThan(1);
        return errors;
    }
    
    public static List<IValidationError> ShouldThrowValidationErrors(this Func<object?> actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBeGreaterThan(1, customMessage);
        return errors;
    }
}