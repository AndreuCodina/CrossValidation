using System.Net;
using CrossValidation.Exceptions;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Exceptions;

public class MessageBusinessExceptionTests
{
    [Fact]
    public void Instantiate_exception()
    {
        var expectedMessage = "Expected message with 1";
        var expectedStatusCode = HttpStatusCode.BadRequest;
        var expectedDetails = "Expected details";
        
        var exception = new CustomMessageBusinessException(1);
        
        exception.Message.ShouldBe(expectedMessage);
        exception.StatusCode.ShouldBe(expectedStatusCode);
        exception.Details.ShouldBe(expectedDetails);
    }

    private class CustomMessageBusinessException(int parameter)
        : MessageBusinessException($"Expected message with {parameter}", HttpStatusCode.BadRequest, "Expected details");
}