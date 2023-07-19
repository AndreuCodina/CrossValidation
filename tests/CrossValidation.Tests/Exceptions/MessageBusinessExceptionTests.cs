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
        var expectedMessage = "Expected message";
        var expectedStatusCode = HttpStatusCode.BadRequest;
        var expectedDetails = "Expected details";
        
        var exception = new MessageBusinessException(expectedMessage, expectedStatusCode, expectedDetails);
        
        exception.Message.ShouldBe(expectedMessage);
        exception.StatusCode.ShouldBe(expectedStatusCode);
        exception.Details.ShouldBe(expectedDetails);
    }
}