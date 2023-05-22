using CrossValidation.Errors;
using CrossValidation.Exceptions;
using Shouldly;

namespace CrossValidation.ShouldlyAssertions;

[ShouldlyMethods]
public static class AssertionExtensions
{
    public static ICrossError ShouldThrowCrossError(this Action actual, string? customMessage = null)
    {
        return actual.ShouldThrow<CrossException>(customMessage).Error;
    }
    
    public static async Task<ICrossError> ShouldThrowCrossErrorAsync(this Task<Action> actual, string? customMessage = null)
    {
        return (await actual.ShouldThrowAsync<CrossException>(customMessage)).Error;
    }
    
    public static ICrossError ShouldThrowCrossError(this Func<object?> actual, string? customMessage = null)
    {
        return actual.ShouldThrow<CrossException>(customMessage).Error;
    }
    
    public static async Task<ICrossError> ShouldThrowCrossErrorAsync(this Func<Task> actual, string? customMessage = null)
    {
        return (await actual.ShouldThrowAsync<CrossException>(customMessage)).Error;
    }
    
    public static TCrossError ShouldThrowCrossError<TCrossError>(this Action actual, string? customMessage = null)
        where TCrossError : ICrossError
    {
        var error = actual.ShouldThrow<CrossException>().Error;
        return error.ShouldBeOfType<TCrossError>(customMessage);
    }
    
    public static TCrossError ShouldThrowCrossError<TCrossError>(this Func<object?> actual, string? customMessage = null)
        where TCrossError : ICrossError
    {
        var error = actual.ShouldThrow<CrossException>().Error;
        return error.ShouldBeOfType<TCrossError>(customMessage);
    }
    
    public static async Task<TCrossError> ShouldThrowCrossErrorAsync<TCrossError>(this Task<Action> actual, string? customMessage = null)
        where TCrossError : ICrossError
    {
        var error = (await actual.ShouldThrowAsync<CrossException>()).Error;
        return error.ShouldBeOfType<TCrossError>(customMessage);
    }
    
    public static TCrossError ShouldThrowCrossError<TCrossError>(this Func<Task> actual, string? customMessage = null)
        where TCrossError : ICrossError
    {
        var error = actual.ShouldThrow<CrossException>().Error;
        return error.ShouldBeOfType<TCrossError>(customMessage);
    }
    
    public static async Task<TCrossError> ShouldThrowCrossErrorAsync<TCrossError>(this Func<Task> actual, string? customMessage = null)
        where TCrossError : ICrossError
    {
        var error = (await actual.ShouldThrowAsync<CrossException>()).Error;
        return error.ShouldBeOfType<TCrossError>(customMessage);
    }
    
    public static List<ICrossError> ShouldThrowCrossErrors(this Action actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<ValidationListException>(customMessage).Errors;
        return errors;
    }
    
    public static async Task<List<ICrossError>> ShouldThrowCrossErrorsAsync(this Task<Action> actual, string? customMessage = null)
    {
        var errors = (await actual.ShouldThrowAsync<ValidationListException>(customMessage)).Errors;
        return errors;
    }
    
    public static List<ICrossError> ShouldThrowCrossErrors(this Func<object?> actual, string? customMessage = null)
    {
        var errors = actual.ShouldThrow<ValidationListException>(customMessage).Errors;
        return errors;
    }
    
    public static async Task<List<ICrossError>> ShouldThrowCrossErrorsAsync(this Func<Task> actual, string? customMessage = null)
    {
        var errors = (await actual.ShouldThrowAsync<ValidationListException>(customMessage)).Errors;
        return errors;
    }
}