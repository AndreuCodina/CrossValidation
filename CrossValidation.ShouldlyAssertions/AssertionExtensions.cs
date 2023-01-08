using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Results;
using Shouldly;

namespace CrossValidation.ShouldlyAssertions;

[ShouldlyMethods]
public static class AssertionExtensions
{
    public static CrossError ShouldThrowError(this Action actual, string? customMessage = null)
    {
        return actual.ShouldThrow<CrossException>(customMessage)
            .Error;
    }
    
    public static TError ShouldThrowError<TError>(this Action actual, string? customMessage = null)
        where TError : CrossError
    {
        return actual.ShouldThrow<CrossException>()
            .Error
            .ShouldBeOfType<TError>(customMessage);
    }
    
    public static CrossError ShouldThrowError(this Func<object?> actual, string? customMessage = null)
    {
        return actual.ShouldThrow<CrossException>(customMessage)
            .Error;
    }

    public static TError ShouldThrowError<TError>(this Func<object?> actual, string? customMessage = null)
        where TError : CrossError
    {
        return actual.ShouldThrow<CrossException>()
            .Error
            .ShouldBeOfType<TError>(customMessage);
    }

    public static ICrossValidationError ShouldThrowValidationError(this Action actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        return errors[0];
    }
    
    public static ICrossValidationError ShouldThrowValidationError(this Func<object?> actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        return errors[0];
    }
    
    public static TValidationError ShouldThrowValidationError<TValidationError>(this Action actual, string? customMessage = null)
        where TValidationError : ICrossValidationError
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        var error = errors[0];
        return error.ShouldBeOfType<TValidationError>(customMessage);
    }
    
    public static TValidationError ShouldThrowValidationError<TValidationError>(this Func<object?> actual, string? customMessage = null)
        where TValidationError : ICrossValidationError
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        var error = errors[0];
        return error.ShouldBeOfType<TValidationError>(customMessage);
    }
    
    public static List<ICrossValidationError> ShouldThrowValidationErrors(this Action actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBeGreaterThan(1);
        return errors;
    }
    
    public static List<ICrossValidationError> ShouldThrowValidationErrors(this Func<object?> actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBeGreaterThan(1, customMessage);
        return errors;
    }
}