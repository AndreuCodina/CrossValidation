using System;
using CrossValidation.Exceptions;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Exceptions;

public class NoStackTraceExceptionTests
{
    [Fact]
    public void Not_generate_stack_trace()
    {
        Action action = () => throw new CustomNoStackTraceException();

        var exception = action.ShouldThrow<CustomNoStackTraceException>();
        exception.StackTrace.ShouldBeNull();
    }

    private class CustomNoStackTraceException : NoStackTraceException
    {
    }
}