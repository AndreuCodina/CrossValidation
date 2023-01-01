using System;
using CrossValidation.Exceptions;
using CrossValidation.Results;
using Shouldly;

namespace CrossValidationTests.TestExtensions;

[ShouldlyMethods]
public static class CustomShouldlyAssertions
{
    public static void ShouldThrowCrossError<TError>(this Action actual, string? customMessage = null)
        where TError : CrossError
    {
        actual.ShouldThrow<CrossException>()
            .Error
            .ShouldBeOfType<TError>();
    }

    public static void ShouldThrowCrossError<TError>(this Func<object?> actual, string? customMessage = null)
        where TError : CrossError
    {
        actual.ShouldThrow<CrossException>(customMessage)
            .Error
            .ShouldBeOfType<TError>(customMessage);
    }
}