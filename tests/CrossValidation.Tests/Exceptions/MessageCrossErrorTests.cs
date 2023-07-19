using CrossValidation.Exceptions;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Exceptions;

public class MessageCrossErrorTests
{
    [Fact]
    public void Create_message_with_parameters()
    {
        var expectedMessage = "The parameters are 1 and foo";
        
        var error = new MessageCrossError("The parameters are {0} and {1}", 1, "foo");

        error.Message.ShouldBe(expectedMessage);
    }
}