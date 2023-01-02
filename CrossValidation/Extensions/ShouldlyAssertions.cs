using CrossValidation.Exceptions;
using CrossValidation.Results;
using Shouldly;

namespace CrossValidation.Extensions;

[ShouldlyMethods]
public static class ShouldlyAssertions
{
    public static TError ShouldThrowError<TError>(this Action actual, string? customMessage = null)
        where TError : CrossError
    {
        return actual.ShouldThrow<CrossException>()
            .Error
            .ShouldBeOfType<TError>(customMessage);
    }

    public static TError ShouldThrowError<TError>(this Func<object?> actual, string? customMessage = null)
        where TError : CrossError
    {
        return actual.ShouldThrow<CrossException>()
            .Error
            .ShouldBeOfType<TError>(customMessage);
    }
}